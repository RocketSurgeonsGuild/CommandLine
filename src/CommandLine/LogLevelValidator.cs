using System;
using System.ComponentModel.DataAnnotations;
using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Validation;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.CommandLine
{
    class LogLevelValidator : IOptionValidator
    {
        public ValidationResult GetValidationResult(CommandOption option, ValidationContext context)
        {
            if (!option.HasValue()) return ValidationResult.Success;

            if (option.Value().Equals("verbose", StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Success;
            }

            if (Enum.TryParse<LogLevel>(option.Value(), true, out var logLevel))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid log level provided");
        }
    }
}
