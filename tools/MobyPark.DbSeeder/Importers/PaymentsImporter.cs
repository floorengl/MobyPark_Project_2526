using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MobyPark_api.Data;
using MobyPark_api.Data.Models;
using MobyPark_api.Enums;
using Microsoft.EntityFrameworkCore;


namespace MobyPark.DbSeeder.Importers;

public static class PaymentsImporter
{
    public static async Task ImportAsync(AppDbContext db, string basePath)
    {
        // Checking if the json file exists.
        var path = Path.Combine(basePath, "payments.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("Payment file not found, skipping.");
            return;
        }
        // Variables
        const int maxToImport = 50_000;
        const int batchSize = 5_000;
        int pInserted = 0, tInserted = 0, skipped = 0, processed = 0, sinceSave = 0;
        await using var fs = File.OpenRead(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Retrieving payments objects.
        await foreach (var o in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(fs, options))
        {
            processed++;
            if (maxToImport > 0 && processed > maxToImport) break;
            // Check required fields are converted.
            if (!o.TryGetProperty("hash", out var hashProp) || hashProp.ValueKind != JsonValueKind.String ||
                !o.TryGetProperty("amount", out var amountProp) || amountProp.ValueKind != JsonValueKind.Number ||
                !amountProp.TryGetDecimal(out var amount))
            {
                skipped++;
                continue;
            }
            // Check has aint empty.
            var hash = hashProp.GetString();
            if (string.IsNullOrWhiteSpace(hash))
            {
                skipped++;
                continue;
            }
            // Creating the Guid for transactionId
            var seed = o.TryGetProperty("transaction", out var t) && t.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(t.GetString())
                    ? t.GetString()!
                    : hash;
            var transactionId = new Guid(
                MD5.HashData(Encoding.UTF8.GetBytes(seed.Trim()))
            );

            // Variables for TransactionData.
            var hasTData = o.TryGetProperty("t_data", out var tData) && tData.ValueKind == JsonValueKind.Object;
            var txAmount =
                hasTData && tData.TryGetProperty("amount", out var tAmt) &&
                tAmt.ValueKind == JsonValueKind.Number && tAmt.TryGetDecimal(out var a)
                    ? a
                    : amount;
            var txDateUtc =
                hasTData && tData.TryGetProperty("date", out var tDate) && tDate.ValueKind == JsonValueKind.String &&
                tDate.GetString() is string rawDate &&
                DateTime.TryParseExact(rawDate, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
                    ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
                    : DateTime.UtcNow;
            var method =
                hasTData && tData.TryGetProperty("method", out var tMethod) && tMethod.ValueKind == JsonValueKind.String
                    ? (tMethod.GetString() ?? "Ideal")
                    : "Ideal";
            var issuer =
                hasTData && tData.TryGetProperty("issuer", out var tIssuer) && tIssuer.ValueKind == JsonValueKind.String
                    ? (tIssuer.GetString() ?? "XYY910HH")
                    : "XYY910HH";
            var bank =
                hasTData && tData.TryGetProperty("bank", out var tBank) && tBank.ValueKind == JsonValueKind.String
                    ? (tBank.GetString() ?? "ABN-NL")
                    : "ABN-NL";
            // Variables for Payment.
            var createdAtUtc =
                o.TryGetProperty("created_at", out var ca) &&
                DateTime.TryParseExact(
                    ca.GetString() is string s && s.Length >= 16 ? s[..16] : null,
                    "dd-MM-yyyy HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dt)
                    ? DateTime.SpecifyKind(dt, DateTimeKind.Utc)
                    : DateTime.UtcNow;
            var status =
                o.TryGetProperty("completed", out var compProp) &&
                compProp.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(compProp.GetString())
                    ? PaymentStatus.Complete
                    : PaymentStatus.Pending;

            // Insert Transaction
            db.Transactions.Add(new TransactionData
            {
                TransactionId = transactionId,
                Amount = txAmount,
                Date = txDateUtc,
                Method = method,
                Issuer = issuer,
                Bank = bank
            });
            tInserted++;
            // Insert Payment
            db.Payments.Add(new Payment
            {
                Amount = amount,
                CreatedAt = createdAtUtc,
                Status = status,
                Hash = hash!,
                TransactionId = transactionId
            });
            pInserted++;
            // Save the batch and clear.
            sinceSave++;
            if (sinceSave >= batchSize)
            {
                await db.SaveChangesAsync();
                db.ChangeTracker.Clear();
                sinceSave = 0;
            }
        }
        // Save changes and clear.
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        // Import Summary.
        Console.WriteLine($"Payments → Payments inserted={pInserted}, Transactions inserted={tInserted}, skipped={skipped}");
    }
}
