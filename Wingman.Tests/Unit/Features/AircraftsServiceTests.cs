using Moq;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Features.Aircrafts.Models;
using Wingman.Api.Features.Aircrafts.Repositories.Interfaces;
using Wingman.Api.Features.Aircrafts.Services;
using Wingman.Tests.Unit.Core;

namespace Wingman.Tests.Unit.Features;

public class AircraftsServiceTests : CommonServiceTests<Aircraft>
{
    private static readonly Mock<IAircraftsRepository> _mockRepo = new Mock<IAircraftsRepository>();

    public AircraftsServiceTests() : base(new AircraftsService(_mockRepo.Object), _mockRepo.As<ICommonRepository<Aircraft>>())
    {
    }
}
