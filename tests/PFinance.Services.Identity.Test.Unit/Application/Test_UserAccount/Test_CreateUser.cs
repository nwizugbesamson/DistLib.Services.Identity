using DistLib;
using DistLib.Domain;
using NSubstitute;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Application.UserAccount;
using PFinance.Services.Identity.Application.UserAccount.Commands.CreateUser;
using PFinance.Services.Identity.Common.Enums;
using PFinance.Services.Identity.Domain.User;
using Shouldly;
using Xunit.Sdk;

namespace PFinance.Services.Identity.Test.Unit.Application.Test_UserAccount;

public class Test_CreateUser
{
    private readonly IUserReadService _userReadService = Substitute.For<IUserReadService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IUserFactory _userFactory = Substitute.For<IUserFactory>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordService _passwordService = Substitute.For<IPasswordService>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateUserCommandHandler _sut;

    public Test_CreateUser()
    {
        _sut = new CreateUserCommandHandler(_userReadService, _userFactory, _userRepository, _passwordService, _dateTimeProvider, _unitOfWork);
    }

    [Theory]
    [InlineData("test.user@pfinance.com", "password123", UserRole.Admin)]
    [InlineData("customer@example.com", "securePass456", UserRole.Customer)]
    public async Task Test_UserCanBeCreated_WhenDataIsValid(string email, string password, UserRole role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hashedPassword = "hashed_password_123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var command = new CreateUser(userId, email, password, role);

        _userReadService.UserExistsByEmailAsync(email).Returns(Task.FromResult(false));
        _passwordService.HashPassword(password).Returns(hashedPassword);
        _dateTimeProvider.UtcNow.Returns(createdAt);
        
        var createdUser = User.Create(Email.Create(email).Value, hashedPassword, role, createdAt);
        _userFactory.CreateUser(email, hashedPassword, role, createdAt).Returns(Result.Success(createdUser));
        _userRepository.CreateAsync(createdUser, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        _unitOfWork.SaveChangesAsync().Returns(Task.CompletedTask);

        // Act
        Result result = await Act(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        await _userReadService.Received(1).UserExistsByEmailAsync(email);
        _passwordService.Received(1).HashPassword(password);
        _userFactory.Received(1).CreateUser(email, hashedPassword, role, createdAt);
        await _userRepository.Received(1).CreateAsync(createdUser, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Theory]
    [InlineData("test.user@pfinance.com", "password123", UserRole.Admin)]
    [InlineData("customer@example.com", "securePass456", UserRole.Customer)]
    public async Task Test_UserCreationFails_WhenUserWithEmailAlreadyExists(string email, string password, UserRole role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new CreateUser(userId, email, password, role);

        _userReadService.UserExistsByEmailAsync(email).Returns(Task.FromResult(true));

        // Act
        Result result = await Act(command);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("USER_ALREADY_EXISTS");
        await _userReadService.Received(1).UserExistsByEmailAsync(email);
        _passwordService.DidNotReceive().HashPassword(Arg.Any<string>());
        _userFactory.DidNotReceive().CreateUser(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<UserRole>(), Arg.Any<DateTime>());
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
    

    [Theory]
    [InlineData("test.user@pfinance.com", "password123", UserRole.Admin)]
    [InlineData("customer@example.com", "securePass456", UserRole.Customer)]
    public async Task Test_UserCreationFails_WhenUserFactoryFails(string email, string password, UserRole role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hashedPassword = "hashed_password_123";
        var createdAt = DateTime.Parse("2025-02-01 10:00:00");
        var command = new CreateUser(userId, email, password, role);

        _userReadService.UserExistsByEmailAsync(email).Returns(Task.FromResult(true));
        _passwordService.HashPassword(password).Returns(hashedPassword);
        _dateTimeProvider.UtcNow.Returns(createdAt);
        _userFactory.CreateUser(email, hashedPassword, role, createdAt).Returns(Result.Failure<User>(new Error("INVALID_EMAIL", "Invalid email format")));

        // Act
        Result result = await Act(command);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("INVALID_EMAIL");
        await _userReadService.Received(1).UserExistsByEmailAsync(email);
        _passwordService.Received(1).HashPassword(password);
        _userFactory.Received(1).CreateUser(email, hashedPassword, role, createdAt);
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

   
    

    // Act
    private async Task<Result> Act(CreateUser cmd) => await _sut.Handle(cmd, CancellationToken.None);
}