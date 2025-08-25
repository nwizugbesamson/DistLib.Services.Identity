using DistLib;
using NSubstitute;
using PFinance.Services.Identity.Application.Services;
using PFinance.Services.Identity.Application.UserAccount;
using PFinance.Services.Identity.Application.UserAccount.Commands.SignInPassword;
using PFinance.Services.Identity.Domain.RefreshToken;

namespace PFinance.Services.Identity.Test.Unit.Application.Test_UserAccount;

public class Test_SignInPassword
{
    private readonly IUserReadService _userReadService =  Substitute.For<IUserReadService>();
    private readonly IDateTimeProvider _dateTimeProvider = Substitute.For<IDateTimeProvider>();
    private readonly IJwtProvider  _jwtProvider = Substitute.For<IJwtProvider>();
    private readonly IRefreshTokenRepository  _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly SignInPasswordCommandHandler _sut;

    public Test_SignInPassword()
    {
        _sut = new (_dateTimeProvider, _jwtProvider, _userReadService, _refreshTokenRepository, _unitOfWork);
    }

    private async Task<Result<AuthDto>> Act(SignInPassword cmd) => await _sut.Handle(cmd, CancellationToken.None);
}