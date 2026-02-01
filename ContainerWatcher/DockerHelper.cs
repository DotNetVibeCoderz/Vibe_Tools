using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace ContainerWatcher
{
    public static class DockerHelper
    {
        public static DockerClient CreateClient()
        {
            // Deteksi OS untuk menentukan endpoint Docker
            Uri dockerUri;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Default named pipe untuk Docker Desktop di Windows
                dockerUri = new Uri("npipe://./pipe/docker_engine");
            }
            else
            {
                // Default unix socket untuk Linux/macOS
                dockerUri = new Uri("unix:///var/run/docker.sock");
            }

            // Membuat konfigurasi client
            var config = new DockerClientConfiguration(dockerUri);
            return config.CreateClient();
        }
    }
}