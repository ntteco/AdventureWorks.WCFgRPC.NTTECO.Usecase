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
using Google.Protobuf.WellKnownTypes;
namespace AdventureWorks.GrpcService
{

    public static class TimestampHelpers
    {
        public static Timestamp? ToTimestamp(this DateTime? dt)
        {
            if (!dt.HasValue) return null;
            var utc = DateTime.SpecifyKind(dt.Value, DateTimeKind.Utc);
            return Timestamp.FromDateTime(utc);
        }

        public static Timestamp ToTimestampNonNull(this DateTime dt)
        {
            var utc = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            return Timestamp.FromDateTime(utc);
        }

        public static DateTime? FromTimestamp(this Timestamp? ts)
        {
            if (ts == null) return null;
            return ts.ToDateTime();
        }
    }
}