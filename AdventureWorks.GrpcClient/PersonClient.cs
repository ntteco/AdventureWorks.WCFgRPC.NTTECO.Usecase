/*
 * Copyright (c) 2026 NTTECO.
 *
 * This source code is provided for demonstration and educational purposes only.
 * It is offered "as is", without warranty of any kind, express or implied.
 *
 * The WCF and gRPC code in this repository is intentionally minimal and may not
 * represent production-ready implementations.
 *
 * **The purpose of this repository is to illustrate the development effort and
 * complexity involved in modernizing WCF services to gRPC. NTTECO is referenced
 * only as a modernization usecase category — not as a migration tool, converter,
 * or automation framework.**
 *
 * NTTECO is the Metadata-Driven Object Graph Authority Platform. No NTTECO
 * implementation is included in this repository.
 *
 * For more information about NTTECO, visit https://ntteco.com.
 */

using System.Threading.Tasks;
using Grpc.Core;
using AdventureWorks.GrpcClient.Protos.Person;

namespace AdventureWorks.GrpcClient
{
    public class PersonClient
    {
        private readonly Channel _channel;
        private readonly PersonService.PersonServiceClient _client;

        public PersonClient(string address)
        {
            _channel = new Channel(address, ChannelCredentials.Insecure);
            _client = new PersonService.PersonServiceClient(_channel);
        }

        public async Task<PersonDto> GetPersonAsync(int personId)
        {
            return await _client.GetPersonAsync(new GetPersonRequest { PersonId = personId });
        }

        public async Task<PagedPeople> ListPeopleAsync(int page, int size, string filter)
        {
            return await _client.ListPeopleAsync(new ListPeopleRequest
            {
                Page = page,
                Size = size,
                Filter = filter ?? ""
            });
        }

        public async Task<PersonXmlDto> GetPersonRawXmlAsync(int personId)
        {
            return await _client.GetPersonRawXmlAsync(new GetPersonRequest { PersonId = personId });
        }

        public async Task ShutdownAsync() => await _channel.ShutdownAsync();
    }
}
