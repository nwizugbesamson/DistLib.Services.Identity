using DistLib;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Domain.User;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.CreateUser;

public class CreateUserCommandHandler(
    IUserReadService userReadService,
    IUserFactory userFactory,
    IUserRepository userRepository,
    IPasswordService passwordService,
    IDateTimeProvider dateTimeProvider,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateUser, Result>
{
    private readonly IUserReadService _userReadService = userReadService;
    private readonly IUserFactory _userFactory = userFactory;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordService _passwordService = passwordService;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result> Handle(CreateUser request, CancellationToken cancellationToken)
    {
        // check that user account with the email does not exist
        bool userExists = await _userReadService.UserExistsByEmailAsync(request.Email);
        if (userExists)
        {
            return Result.Failure(new Error("USER_ALREADY_EXISTS", "User with this email already exists"));
        }
        
        // hash password
        string hashedPassword = _passwordService.HashPassword(request.Password);
        
        // get current timestamp
        DateTime createdAt = _dateTimeProvider.UtcNow;
        
        // create a user account with a factory -> raises an event
        Result<User> createUserResult = _userFactory.CreateUser(request.Email, hashedPassword, request.Role, createdAt);
        if (createUserResult.IsFailure)
        {
            return Result.Failure(createUserResult.Error);
        }
        
        User user = createUserResult.Value;
        
        // save with repository
        await _userRepository.CreateAsync(user, cancellationToken);
        
        // complete transaction with unit of work
        await _unitOfWork.SaveChangesAsync();
        
        return Result.Success();
    }
}