using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Data.SqlClient;
using Renci.SshNet;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.Extensions.Primitives;


class Program
{
    static void Main()
    {
        Console.WriteLine("Starting application...");

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = builder.Build();

        string? connectionString = configuration.GetConnectionString("connstr");
        string? query = configuration["SqlSettings:DailyRevQuery"];
        string? sftpUsername = configuration["AppSettings:SftpUsername"];
        string? sftpPassword = configuration["AppSettings:SftpPassword"];
        string? sftpHost = configuration["AppSettings:SftpHost"];
        string? sftpFolderPath = configuration["AppSettings:SftpFolderPath"];
        string formattedDate = DateTime.Now.ToString("MMddyyyy");
        string localTestFile = "testfile.txt";
        string? sftpFilePath = sftpFolderPath + "/" + localTestFile; //$"/SalesTransaction {formattedDate}.csv";
        


        //File.WriteAllText(localTestFile, "this is a test file.");
        try
        {
            UploadToSftp(localTestFile, sftpHost, sftpUsername, sftpPassword, sftpFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);

        }

        static void UploadToSftp(string localFilePath, string host, string username, string password, string remotePath)
        {
            using (SftpClient client = new(host, username, password))
            {
                client.Connect();

                Console.WriteLine("Connected to SFTP server");

                using (FileStream fs = new(localFilePath, FileMode.Open))
                {
                    client.UploadFile(fs, remotePath);
                }

                Console.WriteLine("File uploaded successfully");

                client.Disconnect();
            }
        }
        

    }
}