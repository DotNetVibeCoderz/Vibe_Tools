using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace ContainerWatcher
{
    public class ContainerService
    {
        private readonly DockerClient _client;

        public ContainerService()
        {
            _client = DockerHelper.CreateClient();
        }

        // Mendapatkan daftar semua container (termasuk yang mati)
        public async Task<IList<ContainerListResponse>> ListContainersAsync()
        {
            try
            {
                var parameters = new ContainersListParameters()
                {
                    All = true // Tampilkan container yang stop juga
                };

                return await _client.Containers.ListContainersAsync(parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Gagal konek ke Docker: {ex.Message}");
                Console.WriteLine("Pastikan Docker Desktop/Engine sudah jalan ya bos!");
                return new List<ContainerListResponse>();
            }
        }

        // Start Container
        public async Task<bool> StartContainerAsync(string containerId)
        {
            try
            {
                // Start container
                return await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Gagal start container {containerId}: {ex.Message}");
                return false;
            }
        }

        // Stop Container
        public async Task<bool> StopContainerAsync(string containerId)
        {
            try
            {
                // Stop container dengan timeout 5 detik
                return await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters { WaitBeforeKillSeconds = 5 });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Gagal stop container {containerId}: {ex.Message}");
                return false;
            }
        }

        // Mendapatkan Logs (50 baris terakhir)
        public async Task<string> GetContainerLogsAsync(string containerId)
        {
            try
            {
                var parameters = new ContainerLogsParameters
                {
                    ShowStdout = true,
                    ShowStderr = true,
                    Tail = "50"
                };

                using (var stream = await _client.Containers.GetContainerLogsAsync(containerId, parameters))
                using (var reader = new System.IO.StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                return $"[Error] Gagal baca logs: {ex.Message}";
            }
        }
    }
}