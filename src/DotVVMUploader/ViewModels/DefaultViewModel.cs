using DotVVM.Framework.Controls;
using DotVVM.Framework.Storage;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using System.Threading.Tasks;

namespace DotVVMUploader.ViewModels
{
    public class DefaultViewModel : MasterPageViewModel
    {
        private readonly IUploadedFileStorage _uploadedFileStorage;
        public string Title { get; set; }

        public UploadedFilesCollection FilesCollection { get; set; }

        public DefaultViewModel(IUploadedFileStorage uploadedFileStorage)
        {
            Title = "Welcome to upload files sample";

            FilesCollection = new UploadedFilesCollection();

            _uploadedFileStorage = uploadedFileStorage;
        }

        public void SaveMyFilesLocally()
        {
            if (!FilesCollection.IsBusy)
            {
                var permenentPath = Path.Combine(Context.Configuration.ApplicationPhysicalPath, "ProcessedFiles");
                if (!Directory.Exists(permenentPath))
                {
                    Directory.CreateDirectory(permenentPath);
                }

                foreach (var file in FilesCollection.Files)
                {
                    var newFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{file.FileId}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(permenentPath, newFileName);
                    _uploadedFileStorage.SaveAs(file.FileId, filePath);
                    _uploadedFileStorage.DeleteFile(file.FileId);
                }
                FilesCollection.Clear();
            }
        }

        public async Task SaveMyFilesToAzure()
        {
            if (!FilesCollection.IsBusy)
            {
                var stoageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                var blobStorageClient = stoageAccount.CreateCloudBlobClient();
                var container = blobStorageClient.GetContainerReference("dotvvmblobcontainer");
                await container.CreateIfNotExistsAsync();

                foreach (var file in FilesCollection.Files)
                {
                    var newFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{file.FileId}{Path.GetExtension(file.FileName)}";
                    var blobRef = container.GetBlockBlobReference(newFileName);
                    await blobRef.UploadFromFileAsync($"temp/uploadedFiles/{file.FileId}.tmp");
                }
                FilesCollection.Clear();
            }
        }
    }
}