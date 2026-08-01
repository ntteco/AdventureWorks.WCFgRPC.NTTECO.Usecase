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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AdventureWorks.GrpcService.Data;
using AdventureWorks.WCFEntities;
using Google.Protobuf.WellKnownTypes;
using PersonProto = AdventureWorks.GrpcService.Protos.Person;

namespace AdventureWorks.GrpcService.Services
{
    public class GrpcPersonService : PersonProto.PersonService.PersonServiceBase
    {
        private readonly IDbContextFactory<AdventureWorks2025Context> _dbFactory;
        private readonly ILogger<GrpcPersonService> _logger;

        public GrpcPersonService(IDbContextFactory<AdventureWorks2025Context> dbFactory, ILogger<GrpcPersonService> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        public override async Task<PersonProto.PersonDto> GetPerson(PersonProto.GetPersonRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));

            await using var db = _dbFactory.CreateDbContext();
            var e = await db.People.AsNoTracking().SingleOrDefaultAsync(p => p.BusinessEntityID == request.PersonId);

            if (e == null) throw new RpcException(new Status(StatusCode.NotFound, $"Person/{request.PersonId} not found"));

            return MapToProto(e);
        }

        public override async Task<PersonProto.PagedPeople> ListPeople(PersonProto.ListPeopleRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));
            if (request.Page <= 0 || request.Size <= 0) throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid paging"));

            await using var db = _dbFactory.CreateDbContext();
            IQueryable<Person> q = db.People.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                var f = request.Filter.Trim();
                q = q.Where(p => p.FirstName.Contains(f) || p.LastName.Contains(f));
            }

            var total = await q.CountAsync();
            var skip = (request.Page - 1) * request.Size;
            var entities = await q.OrderBy(p => p.BusinessEntityID).Skip(skip).Take(request.Size).ToListAsync();

            var result = new PersonProto.PagedPeople { Page = request.Page, Size = request.Size, TotalCount = (int)total };
            result.Items.AddRange(entities.Select(MapToListItemProto));
            return result;
        }

        public override async Task<PersonProto.PersonXmlDto> GetPersonRawXml(PersonProto.GetPersonRequest request, ServerCallContext context)
        {
            if (request == null) throw new RpcException(new Status(StatusCode.InvalidArgument, "Request is null"));

            await using var db = _dbFactory.CreateDbContext();
            var e = await db.People.AsNoTracking().SingleOrDefaultAsync(p => p.BusinessEntityID == request.PersonId);

            if (e == null) throw new RpcException(new Status(StatusCode.NotFound, $"Person/{request.PersonId} not found"));

            return new PersonProto.PersonXmlDto
            {
                PersonId = e.BusinessEntityID,
                AdditionalContactInfoRaw = e.AdditionalContactInfo ?? string.Empty,
                DemographicsRaw = e.Demographics ?? string.Empty,
                Rowguid = e.RowGuid.ToString(),
                ModifiedDate = e.ModifiedDate.ToTimestamp(),
            };
        }

        #region Mapping & XML helpers

        private static PersonProto.PersonDto MapToProto(AdventureWorks.WCFEntities.Person e)
        {
            return new PersonProto.PersonDto
            {
                PersonId = e.BusinessEntityID,
                PersonType = e.PersonType ?? string.Empty,
                NameStyle = e.NameStyle,
                Title = e.Title ?? string.Empty,
                FirstName = e.FirstName ?? string.Empty,
                MiddleName = e.MiddleName ?? string.Empty,
                LastName = e.LastName ?? string.Empty,
                Suffix = e.Suffix ?? string.Empty,
                EmailPromotion = e.EmailPromotion,
                AdditionalContactInfoRaw = e.AdditionalContactInfo ?? string.Empty,
                DemographicsRaw = e.Demographics ?? string.Empty,
                Rowguid = e.RowGuid.ToString(),
                // convert DateTime? to Timestamp? using helper
                ModifiedDate = e.ModifiedDate.ToTimestamp(),
                DemographicsSummary = ExtractDemographicsSummary(e.Demographics) ?? string.Empty,
                PrimaryContact = ExtractPrimaryContact(e.AdditionalContactInfo) ?? string.Empty
            };
        }

        private static PersonProto.PersonListItemDto MapToListItemProto(Person e)
        {
            return new PersonProto.PersonListItemDto
            {
                PersonId = e.BusinessEntityID,
                FirstName = e.FirstName ?? string.Empty,
                LastName = e.LastName ?? string.Empty,
                PrimaryContact = ExtractPrimaryContact(e.AdditionalContactInfo) ?? string.Empty,
                DemographicsSummary = ExtractDemographicsSummary(e.Demographics) ?? string.Empty
            };
        }

        private static string? ExtractPrimaryContact(string? xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xml);
                var email = doc.Descendants("Email").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(email)) return email.Trim();
                var phone = doc.Descendants("Phone").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(phone)) return phone.Trim();
                return doc.Descendants("Contact").FirstOrDefault()?.Value?.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static string? ExtractDemographicsSummary(string? xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            try
            {
                var doc = System.Xml.Linq.XDocument.Parse(xml);
                var parts = new List<string>();
                var age = doc.Descendants("AgeRange").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(age)) parts.Add($"Age:{age.Trim()}");
                var hh = doc.Descendants("HouseholdSize").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(hh)) parts.Add($"Household:{hh.Trim()}");
                var seg = doc.Descendants("Segment").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(seg)) parts.Add($"Segment:{seg.Trim()}");
                if (parts.Count == 0) return doc.Root?.Elements().FirstOrDefault()?.Value?.Trim();
                return string.Join("; ", parts);
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
