using kolekt.Data;
using kolekt.Data.Models;
using kolekt.EventSourcing.Messages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using Xunit;

namespace kolekt.EventSourcing.UnitTests
{
    public class EventStoreDbFixture : IDisposable
    {
        public EventStoreDataContext @DbContext { get; private set; }
        public Guid AggregateId { get; private set; }

        public EventStoreDbFixture()
        {
            var dbOptions = new DbContextOptionsBuilder<EventStoreDataContext>()
                .UseInMemoryDatabase("Events")
                .Options;

            @DbContext = new EventStoreDataContext(dbOptions);

            AggregateId = Guid.NewGuid();
            Seed();
        }

        private void Seed()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();

            int version = 1;
            for (int i = 0; i < 10; i++)
            {
                var e = new EventEntity
                {
                    AggregateId = AggregateId,
                    AggregateType = typeof(MockAggregate).Name,
                    Data = System.Text.Json.JsonSerializer.Serialize(new MockEvent()),
                    Id = Guid.NewGuid(),
                    Name = $"{typeof(MockEvent).FullName}, {typeof(MockEvent).Assembly.FullName}",
                    Version = version + i,
                };
                DbContext.Events.Add(e);
            }
            DbContext.SaveChanges();
        }

        #region IDisposable
         
        private bool _isDisposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            if (disposing)
            {
                @DbContext.Dispose();

                _isDisposed = true;
            }
        }

        #endregion
    }
}
