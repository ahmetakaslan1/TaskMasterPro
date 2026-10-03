using AutoMapper;
using FluentValidation;
using TaskMasterPro.Application.DTOs;
using TaskMasterPro.Application.Exceptions;
using TaskMasterPro.Application.Interfaces;
using TaskMasterPro.Domain.Entities;
using TaskMasterPro.Domain.Interfaces;

namespace TaskMasterPro.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider,
        IMapper mapper,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _mapper = mapper;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Exceptions.ValidationException(validationResult.Errors);
        }

        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser != null)
        {
            throw new Exceptions.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Email", "Email is already in use.") });
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            
            var board = new Board
            {
                Title = "İlk Panom",
                User = user
            };
            await _unitOfWork.Boards.AddAsync(board, cancellationToken);
            
            var col1 = new BoardColumn { Title = "Yapılacaklar", OrderIndex = 0, Board = board };
            var col2 = new BoardColumn { Title = "Devam Edenler", OrderIndex = 1, Board = board };
            var col3 = new BoardColumn { Title = "Bitenler", OrderIndex = 2, Board = board };
            
            await _unitOfWork.BoardColumns.AddAsync(col1, cancellationToken);
            await _unitOfWork.BoardColumns.AddAsync(col2, cancellationToken);
            await _unitOfWork.BoardColumns.AddAsync(col3, cancellationToken);
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        var token = _jwtProvider.GenerateToken(user);
        return new AuthResponseDto { Token = token, User = _mapper.Map<UserDto>(user) };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new Exceptions.ValidationException(validationResult.Errors);
        }

        var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = _jwtProvider.GenerateToken(user);
        return new AuthResponseDto { Token = token, User = _mapper.Map<UserDto>(user) };
    }

    public async Task<UserDto> GetMeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        return _mapper.Map<UserDto>(user);
    }
}
