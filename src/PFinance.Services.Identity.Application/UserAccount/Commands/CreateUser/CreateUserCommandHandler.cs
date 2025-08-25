using DistLib;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Domain.User;

namespace PFinance.Services.Identity.Application.UserAccount.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUser, Result>
{
    private readonly IUserReadService _userReadService;
    private readonly IUserFactory _userFactory;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(IUserReadService userReadService, IUserFactory userFactory,
        IUserRepository userRepository, IPasswordService passwordService, IDateTimeProvider dateTimeProvider,
        IUnitOfWork unitOfWork)
    {
        _userReadService = userReadService;
        _userFactory = userFactory;
        _userRepository = userRepository;
        _passwordService = passwordService;
        _dateTimeProvider = dateTimeProvider;
        _unitOfWork = unitOfWork;
    }

    public Task<Result> Handle(CreateUser request, CancellationToken cancellationToken)
    {
        // check that user account with the email does not exist
        
        // hash password
        
        // create a user account with a factory -> raises an event
        
        // save with repository
        
        // complete transaction with unit of work
        throw new NotImplementedException();
    }
}