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
using AdventureWorks.WCFEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Xml.Linq;

namespace AdventureWorks.WCFServices
{
    public class PersonService : IPersonService
    {
        // NOTE: using a short-lived context per call keeps the demo simple.
        // In a real service you might inject the context or use a factory.
        public PersonDto GetPerson(GetPersonRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using (var ctx = new AdventureWorks2025Context())
            {
                var entity = ctx.People
                                .SingleOrDefault(p => p.BusinessEntityID == request.PersonId);

                if (entity == null)
                {
                    var nf = new NotFoundFault
                    {
                        Resource = $"Person/{request.PersonId}",
                        Message = "Person not found."
                    };
                    throw new FaultException<NotFoundFault>(nf, new FaultReason(nf.Message));
                }

                var dto = MapToPersonDto(entity);
                return dto;
            }
        }

        public PersonXmlDto GetPersonRawXml(GetPersonRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            using (var ctx = new AdventureWorks2025Context())
            {
                var entity = ctx.People
                                .SingleOrDefault(p => p.BusinessEntityID == request.PersonId);

                if (entity == null)
                {
                    var nf = new NotFoundFault
                    {
                        Resource = $"Person/{request.PersonId}",
                        Message = "Person not found."
                    };
                    throw new FaultException<NotFoundFault>(nf, new FaultReason(nf.Message));
                }

                return new PersonXmlDto
                {
                    BusinessEntityID = entity.BusinessEntityID,
                    AdditionalContactInfoRaw = entity.AdditionalContactInfo,
                    DemographicsRaw = entity.Demographics,
                    RowGuid = entity.RowGuid,
                    ModifiedDate = entity.ModifiedDate
                };
            }
        }

        public PagedPeopleDto ListPeople(ListPeopleRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Basic validation
            if (request.Page <= 0 || request.Size <= 0)
            {
                var vf = new ValidationFault
                {
                    Errors = new List<ValidationError>
                    {
                        new ValidationError { Field = "Page/Size", Message = "Page and Size must be positive integers." }
                    }
                };
                throw new FaultException<ValidationFault>(vf, new FaultReason("Invalid paging parameters."));
            }

            using (var ctx = new AdventureWorks2025Context())
            {
                // Base query
                IQueryable<Person> q = ctx.People;

                // Simple filter semantics: if Filter provided, search FirstName or LastName contains (case-insensitive)
                if (!string.IsNullOrWhiteSpace(request.Filter))
                {
                    var f = request.Filter.Trim();
                    q = q.Where(p => p.FirstName.Contains(f) || p.LastName.Contains(f));
                }

                var total = q.Count();

                var skip = (request.Page - 1) * request.Size;
                var items = q.OrderBy(p => p.BusinessEntityID)
                             .Skip(skip)
                             .Take(request.Size)
                             .ToList()
                             .Select(MapToPersonListItemDto)
                             .ToList();

                return new PagedPeopleDto
                {
                    Items = items,
                    Page = request.Page,
                    Size = request.Size,
                    TotalCount = total
                };
            }
        }

        #region Mapping & XML helpers

        private static PersonDto MapToPersonDto(Person e)
        {
            var dto = new PersonDto
            {
                BusinessEntityID = e.BusinessEntityID,
                PersonType = e.PersonType,
                NameStyle = e.NameStyle,
                Title = e.Title,
                FirstName = e.FirstName,
                MiddleName = e.MiddleName,
                LastName = e.LastName,
                Suffix = e.Suffix,
                EmailPromotion = e.EmailPromotion,
                AdditionalContactInfoRaw = e.AdditionalContactInfo,
                DemographicsRaw = e.Demographics,
                RowGuid = e.RowGuid,
                ModifiedDate = e.ModifiedDate
            };

            // Derived convenience fields
            dto.PrimaryContact = ExtractPrimaryContact(e.AdditionalContactInfo);
            dto.DemographicsSummary = ExtractDemographicsSummary(e.Demographics);

            return dto;
        }

        private static PersonListItemDto MapToPersonListItemDto(Person e)
        {
            return new PersonListItemDto
            {
                BusinessEntityID = e.BusinessEntityID,
                FirstName = e.FirstName,
                LastName = e.LastName,
                PrimaryContact = ExtractPrimaryContact(e.AdditionalContactInfo),
                DemographicsSummary = ExtractDemographicsSummary(e.Demographics)
            };
        }

        /// <summary>
        /// Attempts to extract a preferred contact (email then phone) from the AdditionalContactInfo XML.
        /// Returns null if nothing found or XML invalid.
        /// Expected XML shapes vary; this helper is defensive.
        /// </summary>
        private static string ExtractPrimaryContact(string additionalContactInfoXml)
        {
            if (string.IsNullOrWhiteSpace(additionalContactInfoXml)) return null;

            try
            {
                var doc = XDocument.Parse(additionalContactInfoXml);

                // Common patterns: <Contact><Email>...</Email></Contact> or <Contact><Phone>...</Phone></Contact>
                var email = doc.Descendants("Email").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(email)) return email.Trim();

                var phone = doc.Descendants("Phone").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(phone)) return phone.Trim();

                // Fallback: look for any element named 'Contact' with text
                var contact = doc.Descendants("Contact").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(contact)) return contact.Trim();

                return null;
            }
            catch
            {
                // Invalid XML or unexpected shape — swallow and return null for demo simplicity
                return null;
            }
        }

        /// <summary>
        /// Extracts a short demographics summary from the Demographics XML.
        /// Looks for AgeRange, HouseholdSize, or Segment nodes and composes a small summary.
        /// </summary>
        private static string ExtractDemographicsSummary(string demographicsXml)
        {
            if (string.IsNullOrWhiteSpace(demographicsXml)) return null;

            try
            {
                var doc = XDocument.Parse(demographicsXml);

                var parts = new List<string>();

                var age = doc.Descendants("AgeRange").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(age)) parts.Add($"Age:{age.Trim()}");

                var hh = doc.Descendants("HouseholdSize").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(hh)) parts.Add($"Household:{hh.Trim()}");

                var seg = doc.Descendants("Segment").FirstOrDefault()?.Value;
                if (!string.IsNullOrWhiteSpace(seg)) parts.Add($"Segment:{seg.Trim()}");

                if (parts.Count == 0)
                {
                    // Try any top-level element text as a fallback
                    var firstText = doc.Root?.Elements().FirstOrDefault()?.Value;
                    if (!string.IsNullOrWhiteSpace(firstText)) return firstText.Trim();
                    return null;
                }

                return string.Join("; ", parts);
            }
            catch
            {
                // Invalid XML or unexpected shape — return null for demo simplicity
                return null;
            }
        }

        #endregion
    }
}
