using System;
using System.Security.Cryptography.X509Certificates;
using Azure;
using Azure.Storage.Blobs;
using Azure.Identity;
using System.IO;
using Serilog;

namespace backupBlob {
    class Program {
        static void Main(string[] args) {
            // Configure the logger for console and file output with timestamps
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {SourceContext} {Message}{NewLine}{Exception}")
                .WriteTo.File("logs\\ManufacturingBackup.txt", rollingInterval: RollingInterval.Day,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {SourceContext} {Message}{NewLine}{Exception}")
                .CreateLogger();

            // Log that program started
            Log.Information("Backup program has started");

            try {
                // Define the parameters for the blob upload
                string storageAccountURL = "https://storethantest.blob.core.windows.net/";
                string filePath = "C:\\Users\\ehuynh\\OneDrive - ORBCOMM\\Pictures\\Saved Pictures\\cat.jpg";
                string containerName = "test";
                string blobName = "cat.jpg";
                string certificatePath = "C:\\Development\\backuptest\\ManufacturingBackupCert.pfx";
                string certificatePassword = "KneeShapingCanalTabloid";
                string clientId = "1beca500-d0d9-448d-b8ca-eac6b89e364b";
                string tenantId = "846f6088-bda0-427c-b0fe-804212b3562e";

                // Load the client certificate
                X509Certificate2 clientCertificate = new X509Certificate2(certificatePath, certificatePassword);

                // Create a client certificate credential for authentication
                ClientCertificateCredential credential = new ClientCertificateCredential(tenantId, clientId, clientCertificate);

                // Call the method to upload the file to the blob
                if (UploadFileToBlob(storageAccountURL, containerName, filePath, blobName, credential) == 0) {
                    Log.Information($"Successfully backed up {blobName}");
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"An error occurred in Main: {ex.Message}");
            }

            // Log that program is qutting
            Log.Information("Backup program finished, now quiting");

            // Close and flush the log to write all pending entries
            Log.CloseAndFlush();

            // Prevent the console window from closing immediately
            Console.ReadKey();
        }

        /// <summary>
        /// Uploads a file to an Azure blob storage.
        /// </summary>
        /// <param name="storageAccountURL">The URL of the storage account where the blob resides.</param>
        /// <param name="containerName">The name of the container where the blob will be uploaded.</param>
        /// <param name="filePath">The path of the file to be uploaded.</param>
        /// <param name="blobName">The name of the blob within the container.</param>
        /// <param name="credential">The client certificate credential used for authentication.</param>
        /// <returns>Returns 0 if the upload is successful, and -1 if there's an error.</returns>
        public static int UploadFileToBlob(string storageAccountURL, string containerName, string filePath, string blobName, ClientCertificateCredential credential) {
            int result = 0; // Default result indicating success

            try {
                Log.Information($"Start uploading file {filePath} to blob {blobName} in container {containerName}");

                // Create BlobServiceClient and BlobContainerClient objects
                BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(storageAccountURL), credential);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Get a reference to the blob
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                // Open the file and upload it
                using FileStream uploadFileStream = File.OpenRead(filePath);
                blobClient.Upload(uploadFileStream, true);
                Log.Information($"Successfully uploaded file {filePath} to blob {blobName}");
            }
            catch (Exception ex) {
                Log.Error(ex, $"An error occurred while uploading the file: {ex.Message}");
                Console.WriteLine(ex.Message);
                result = -1; // Indicate failure
            }

            return result; // Return the result of the operation
        }
    }
}
