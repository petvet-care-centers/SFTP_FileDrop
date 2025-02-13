using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFTP_FileDrop.Classes
{
    class CsvFileGeneration
    {
        private readonly IConfiguration _configuration;

        public CsvFileGeneration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Run()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            string? connectionString = _configuration.GetConnectionString("connstr");
            string? query = configuration["SqlSettings:DailyRevQuery"];
            string? sftpUsername = configuration["AppSettings:SftpUsername"];
            string? sftpPassword = configuration["AppSettings:SftpPassword"];
            string? sftpHost = configuration["AppSettings:SftpHost"];
            string? sftpFolderPath = configuration["AppSettings:SftpFolderPath"];
            string formattedDate = DateTime.Now.ToString("MMddyyyy");
            string localFileName = $"SalesTransaction {formattedDate}.csv";
            string? sftpFilePath = sftpFolderPath + "/" + localFileName;

            try
            {
                DataTable dataTable = GenerateDataTable(connectionString, query);
                try
                {
                    ConvertDataTableToCsv(localFileName, dataTable);

                    try
                    {
                        UploadToSftp(localFileName, sftpHost, sftpUsername, sftpPassword, sftpFilePath);
                    }
                    catch (Exception ex)
                    {
                        Log.Exception("UploadToSftp", ex);

                    }
                }
                catch (Exception ex)
                {
                    Log.Exception("ConvertDataTableToCsv", ex);
                }


            }
            catch (Exception ex)
            {
                Log.Exception("GenerateDataTable", ex);
            }

        }

        static DataTable GenerateDataTable(string connString, string sqlQuery)
        {
            var dataTable = new DataTable();

            try
            {
                using (var conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(sqlQuery, conn))
                    using (var reader = command.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dataTable;

        }

        static void ConvertDataTableToCsv(string localFilePath, DataTable dt)
        {
            var csvContent = new StringBuilder();
            foreach (System.Data.DataColumn col in dt.Columns)
            {
                csvContent.Append(col.ColumnName + ",");
            }
            csvContent.Length--;
            csvContent.AppendLine();

            foreach (DataRow row in dt.Rows)
            {
                foreach (var item in row.ItemArray)
                {
                    csvContent.Append(item.ToString() + ",");
                }

                csvContent.Length--;
                csvContent.AppendLine();
            }

            File.WriteAllText(localFilePath, csvContent.ToString(), new UTF8Encoding(false));

        }

        static void UploadToSftp(string localFilePath, string host, string username, string password, string remotePath)
        {
            using (SftpClient client = new(host, username, password))
            {
                client.Connect();

                Log.Info("Connected to SFTP server");

                using (FileStream fs = new(localFilePath, FileMode.Open))
                {
                    client.UploadFile(fs, remotePath);
                }

                Log.Info("File uploaded successfully");

                client.Disconnect();
            }
        }
    }
}
