﻿using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Docker.DotNet
{
    internal class ContainerOperations : IContainerOperations
    {
        internal static readonly ApiResponseErrorHandlingDelegate NoSuchContainerHandler = (statusCode, responseBody) =>
        {
            if (statusCode == HttpStatusCode.NotFound)
            {
                throw new DockerContainerNotFoundException(statusCode, responseBody);
            }
        };

        private readonly DockerClient _client;

        internal ContainerOperations(DockerClient client)
        {
            this._client = client;
        }

        public async Task<IList<ContainerListResponse>> ListContainersAsync(ContainersListParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainersListParameters>(parameters);
            var response = await this._client.MakeRequestAsync(this._client.NoErrorHandlers, HttpMethod.Get, "containers/json", queryParameters, cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<ContainerListResponse[]>(response.Body);
        }

        public async Task<ContainerInspectResponse> InspectContainerAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Get, $"containers/{id}/json", cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<ContainerInspectResponse>(response.Body);
        }

        public async Task<CreateContainerResponse> CreateContainerAsync(CreateContainerParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            IQueryString qs = null;

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (!string.IsNullOrEmpty(parameters.Name))
            {
                qs = new QueryString<CreateContainerParameters>(parameters);
            }

            var data = new JsonRequestContent<CreateContainerParameters>(parameters, this._client.JsonSerializer);
            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, "containers/create", qs, data, cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<CreateContainerResponse>(response.Body);
        }

        public Task<Stream> ExportContainerAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return this._client.MakeRequestForStreamAsync(new[] { NoSuchContainerHandler }, HttpMethod.Get, $"containers/{id}/export", cancellationToken);
        }

        public async Task<ContainerProcessesResponse> ListProcessesAsync(string id, ContainerListProcessesParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainerListProcessesParameters>(parameters);
            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Get, $"containers/{id}/top", queryParameters, cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<ContainerProcessesResponse>(response.Body);
        }

        public async Task<IList<ContainerFileSystemChangeResponse>> InspectChangesAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Get, $"containers/{id}/changes", cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<ContainerFileSystemChangeResponse[]>(response.Body);
        }

        public async Task<bool> StartContainerAsync(string id, ContainerStartParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var queryParams = parameters == null ? null : new QueryString<ContainerStartParameters>(parameters);
            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/start", queryParams, cancellationToken).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotModified;
        }

        public async Task<ContainerExecCreateResponse> ExecCreateContainerAsync(string id, ContainerExecCreateParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var data = new JsonRequestContent<ContainerExecCreateParameters>(parameters, this._client.JsonSerializer);
            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/exec", null, data, cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<ContainerExecCreateResponse>(response.Body);
        }

        public async Task<bool> StopContainerAsync(string id, ContainerStopParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainerStopParameters>(parameters);
            // since specified wait timespan can be greater than HttpClient's default, we set the
            // client timeout to infinite and provide a cancellation token.
            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/stop", queryParameters, null, null, TimeSpan.FromMilliseconds(Timeout.Infinite), cancellationToken).ConfigureAwait(false);
            return response.StatusCode != HttpStatusCode.NotModified;
        }

        public Task RestartContainerAsync(string id, ConatinerRestartParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }


            IQueryString queryParameters = new QueryString<ConatinerRestartParameters>(parameters);
            // since specified wait timespan can be greater than HttpClient's default, we set the
            // client timeout to infinite and provide a cancellation token.
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/restart", queryParameters, null, null, TimeSpan.FromMilliseconds(Timeout.Infinite), cancellationToken);
        }

        public Task KillContainerAsync(string id, ContainerKillParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainerKillParameters>(parameters);
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/kill", queryParameters, cancellationToken);
        }

        public Task PauseContainerAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/pause", cancellationToken);
        }

        public Task UnpauseContainerAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/unpause", cancellationToken);
        }

        public async Task<ContainerWaitResponse> WaitContainerAsync(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/wait", null, null, null, TimeSpan.FromMilliseconds(Timeout.Infinite), cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<ContainerWaitResponse>(response.Body);
        }

        public Task RemoveContainerAsync(string id, ContainerRemoveParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainerRemoveParameters>(parameters);
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Delete, $"containers/{id}", queryParameters, cancellationToken);
        }

        public Task<Stream> GetContainerLogsAsync(string id, ContainerLogsParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainerLogsParameters>(parameters);
            return this._client.MakeRequestForStreamAsync(new[] { NoSuchContainerHandler }, HttpMethod.Get, $"containers/{id}/logs", queryParameters, cancellationToken);
        }

        public Task GetContainerLogsAsync(string id, ContainerLogsParameters parameters, CancellationToken cancellationToken, IProgress<string> progress)
        {
            return StreamUtil.MonitorStreamAsync(
                GetContainerLogsAsync(id, parameters, cancellationToken),
                this._client,
                cancellationToken,
                progress);
        }

        public async Task<GetArchiveFromContainerResponse> GetArchiveFromContainerAsync(string id, GetArchiveFromContainerParameters parameters, bool statOnly, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<GetArchiveFromContainerParameters>(parameters);


            var response = await this._client.MakeRequestForStreamedResponseAsync(new[] { NoSuchContainerHandler }, statOnly ? HttpMethod.Head : HttpMethod.Get, $"containers/{id}/archive", queryParameters, cancellationToken);

            var statHeader = response.Headers.GetValues("X-Docker-Container-Path-Stat").First();

            var bytes = Convert.FromBase64String(statHeader);

            var stat = Encoding.UTF8.GetString(bytes, 0, bytes.Length);

            var pathStat = this._client.JsonSerializer.DeserializeObject<ContainerPathStatResponse>(stat);

            return new GetArchiveFromContainerResponse
            {
                Stat = pathStat,
                Stream = statOnly ? null : response.Body
            };
        }

        public Task ExtractArchiveToContainerAsync(string id, ContainerPathStatParameters parameters, Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainerPathStatParameters>(parameters);

            var data = new BinaryRequestContent(stream, "application/x-tar");
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Put, $"containers/{id}/archive", queryParameters, data, cancellationToken);
        }

        public async Task<MultiplexedStream> AttachContainerAsync(string id, bool tty, ContainerAttachParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var queryParameters = new QueryString<ContainerAttachParameters>(parameters);
            var stream = await this._client.MakeRequestForHijackedStreamAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/attach", queryParameters, null, null, cancellationToken).ConfigureAwait(false);
            if (!stream.CanCloseWrite)
            {
                stream.Dispose();
                throw new NotSupportedException("Cannot shutdown write on this transport");
            }

            return new MultiplexedStream(stream, !tty);
        }

        public Task ResizeContainerTtyAsync(string id, ContainerResizeParameters parameters, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var queryParameters = new QueryString<ContainerResizeParameters>(parameters);
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/resize", queryParameters, cancellationToken);
        }

        // StartContainerExecAsync will start the process specified by id in detach mode with no connected
        // stdin, stdout, or stderr pipes.
        public Task StartContainerExecAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var parameters = new ContainerExecStartParameters
            {
                Detach = true,
            };
            var data = new JsonRequestContent<ContainerExecStartParameters>(parameters, this._client.JsonSerializer);
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"exec/{id}/start", null, data, cancellationToken);
        }

        // StartAndAttachContainerExecAsync will start the process specified by id with stdin, stdout, stderr
        // connected, and optionally using terminal emulation if tty is true.
        public async Task<MultiplexedStream> StartAndAttachContainerExecAsync(string id, bool tty, CancellationToken cancellationToken)
        {
            return await StartWithConfigContainerExecAsync(id, new ExecConfig() { AttachStdin = true, AttachStderr = true, AttachStdout = true, Tty = tty }, cancellationToken);
        }

        public async Task<MultiplexedStream> StartWithConfigContainerExecAsync(string id, ExecConfig eConfig, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var data = new JsonRequestContent<ContainerExecStartParameters>(new ContainerExecStartParameters(eConfig), this._client.JsonSerializer);
            var stream = await this._client.MakeRequestForHijackedStreamAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"exec/{id}/start", null, data, null, cancellationToken).ConfigureAwait(false);
            if (!stream.CanCloseWrite)
            {
                stream.Dispose();
                throw new NotSupportedException("Cannot shutdown write on this transport");
            }

            return new MultiplexedStream(stream, !eConfig.Tty);
        }

        public async Task<ContainerExecInspectResponse> InspectContainerExecAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var response = await this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Get, $"exec/{id}/json", null, cancellationToken).ConfigureAwait(false);
            return this._client.JsonSerializer.DeserializeObject<ContainerExecInspectResponse>(response.Body);
        }

        public Task ResizeContainerExecTtyAsync(string id, ContainerResizeParameters parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var queryParameters = new QueryString<ContainerResizeParameters>(parameters);
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"exec/{id}/resize", queryParameters, cancellationToken);
        }

        public Task<Stream> GetContainerStatsAsync(string id, ContainerStatsParameters parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            IQueryString queryParameters = new QueryString<ContainerStatsParameters>(parameters);
            return this._client.MakeRequestForStreamAsync(this._client.NoErrorHandlers, HttpMethod.Get, $"containers/{id}/stats", queryParameters, null, null, cancellationToken);
        }

        public Task GetContainerStatsAsync(string id, ContainerStatsParameters parameters, IProgress<JSONMessage> progress, CancellationToken cancellationToken = default(CancellationToken))
        {
            return StreamUtil.MonitorStreamForMessagesAsync(
                GetContainerStatsAsync(id, parameters, cancellationToken),
                this._client,
                cancellationToken,
                progress);
        }

        public Task RenameContainerAsync(string id, ContainerRenameParameters parameters, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id));
            }

            var queryParameters = new QueryString<ContainerRenameParameters>(parameters ?? throw new ArgumentNullException(nameof(parameters)));
            return this._client.MakeRequestAsync(new[] { NoSuchContainerHandler }, HttpMethod.Post, $"containers/{id}/rename", queryParameters, cancellationToken);
        }

    }
}