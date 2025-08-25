using DistLib;
using NSubstitute;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Application.UserAccount.Commands.CreateUser;
using PFinance.Services.Identity.Domain.User;

namespace PFinance.Services.Identity.Test.Unit.Application.Test_UserAccount;

public class Test_CreateUser
{
    // check that user can be created with valid data
    
    
    // check that it fails when a user with the same email already exists in system
    
    
    
    // Act
    private async Task<Result> Act(CreateUser cmd) => await _sut.Handle(cmd, CancellationToken.None);
    
    // ARRANGE
    public Test_CreateUser()
    {
        _sut = new (_userReadService, _userFactory, _userRepository, _passwordService,_dateTimeProvider, _unitOfWork);
        
    }

    private readonly IUserReadService _userReadService =  Substitute.For<IUserReadService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUserFactory _userFactory = Substitute.For<IUserFactory>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateUserCommandHandler _sut;
    


}