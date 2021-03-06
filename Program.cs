//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.DataMovement;
using System.Threading.Tasks;

namespace Backup2Azure
{

    /// <summary>
    /// Azure Storage Data Movement Library on CoreCLR targetting Linux platforms - Demonstrates how to copy
    /// and download large directory of files to/from Azure Storage Blob service.
    ///
    ///
    /// Documentation References:
	/// Introduction to Microsoft Azure Storage - https://azure.microsoft.com/en-us/documentation/articles/storage-introduction/
	/// File Storage Concepts - https://msdn.microsoft.com/library/dn166972.aspx
	/// Get Started with Files for C++ - https://azure.microsoft.com/en-us/documentation/articles/storage-c-plus-plus-how-to-use-files/
    /// </summary>

    public class Program
    {

        // *************************************************************************************************************************
        // Instructions: This sample can only be run using an Azure Storage Account. When the sample is run, the user will be
        // prompted to enter an Azure Storage account name and a blob service or an account SAS key that has read/write access to 
        // the blob service.
        //
        // To run the sample you need to supplement two arguments in the Linux commandline
        //      1. First argument is the option to backup or restore data. Simply provide 'backup' or 'restore' keyword
        //      2. Local directory to be backed up or restored to. Full path is needed
        //
        // Sample usage:
        //              dotnet run  <first argument: backup/restore> <second argument: /home/sampledirectory>
        //              dotnet run backup /home/sampledirectory
        //              dotnet run restore /home/sampledirectory
        //
        // *************************************************************************************************************************

        public static int Main(string[] args)
        {

            string connectionStringWithKey = "";
            string connectionString;

            if (args.Length < 2)
            {
                System.Console.WriteLine("Please enter the following parameters.");
                System.Console.WriteLine("Usage: <appname> <backup/restore> <local directory path>");
                System.Console.WriteLine("e.g.: Backup2Azure.exe backup /home/");

                return 1;
            }

            // Use ConnectionString if set, if not prompt for SAS token
            if (connectionStringWithKey == "") { 

                Console.WriteLine("Please enter your account name and then hit enter.");
                string accountName = Console.ReadLine();
                Console.WriteLine("Please enter an account level SAS key generated by your Azure Administrator.");
                Console.WriteLine("e.g. ?sv=2016-05-31&ss=bfqt&srt=sco&sp=rwdl&st=2017-04-24T17%3A13%3A00Z&se=2017-04-25T17%3A13%3A00Z&sig=usADJWPXWVct%2Fxxw8GTSOVv2t7eC7TtnxKeINvu72YM%3D");
                string sasKey = Console.ReadLine();

                connectionString = "BlobEndpoint=https://" + accountName + ".blob.core.windows.net;SharedAccessSignature=" + sasKey;

            } else
            {
                connectionString = connectionStringWithKey;
            }

            DirectoryInfo localDir = new DirectoryInfo(args[1]);

            if (args[0] == "restore")
            {

                Console.WriteLine("Please type the containername you want to restore from:");

                Storage connect = new Storage();
                var task = connect.ListContainers(connectionString);
                task.Wait();

                string containername = Console.ReadLine();

                Download downloadFromAzure = new Download();
                var downloadtask = downloadFromAzure.doDownload(containername, localDir, connectionString);
                downloadtask.Wait();

            }
            else if (args[0] == "backup")
            {

                Upload upload2Azure = new Upload();
                var copytask = upload2Azure.doCopy(localDir, connectionString);
                copytask.Wait();

            }

            Console.ReadLine();
            return 0;

        }

        // Callback when the file transferfails
        public static void FileFailedCallback(object sender, TransferEventArgs e)
        {
            Console.WriteLine("Transfer fails. {0} -> {1}. Error message:{2}", e.Source, e.Destination, e.Exception.Message);
        }

    }
}
