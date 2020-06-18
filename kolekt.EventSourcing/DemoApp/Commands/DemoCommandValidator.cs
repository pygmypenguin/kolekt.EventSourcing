using DemoApp.Aggregates;
using kolekt.EventSourcing.Aggregates;
using kolekt.EventSourcing.Consumers;
using kolekt.EventSourcing.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApp.Messages
{

    public class DemoCommandValidator : CommandValidatorBase
    {
        private readonly IAggregateRepository<DemoAggregate> _aggregateRepository;
        public DemoCommandValidator(IAggregateRepository<DemoAggregate> aggregateRepository)
        {
            _aggregateRepository = aggregateRepository;
        }

        private async Task<CommandValidationResult> Validate(CreateAggregateCommand command)
        {
            bool exists = await _aggregateRepository.FindById(command.DemoId) != null;

            return new CommandValidationResult
            {
                IsValid = exists == false,
                ErrorMessage = exists ? $"Demo with id { command.DemoId } already exists" : string.Empty,
                Property = exists ? nameof(command.DemoId) : string.Empty
            };
        }

        private async Task<CommandValidationResult> Validate(DoSomethingCommand command)
        {
            bool exists = await _aggregateRepository.FindById(command.DemoId) != null;

            return new CommandValidationResult
            {
                IsValid = exists,
                ErrorMessage = !exists ? $"Demo with id { command.DemoId } does not exist" : string.Empty,
                Property = !exists ? nameof(command.DemoId) : string.Empty
            };
        }

        private async Task<CommandValidationResult> Validate(DeleteAggregateCommand command)
        {
            bool exists = await _aggregateRepository.FindById(command.DemoId) != null;

            return new CommandValidationResult
            {
                IsValid = exists,
                ErrorMessage = !exists ? $"Demo with id { command.DemoId } does not exist" : string.Empty,
                Property = !exists ? nameof(command.DemoId) : string.Empty
            };
        }
    }
}
