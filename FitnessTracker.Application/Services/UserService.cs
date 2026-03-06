using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessTracker.Application.Interfaces;
using FitnessTracker.Domain.Entities;
using FitnessTracker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FitnessTracker.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(long id)
    {
        _logger.LogDebug("Getting user by id {UserId}", id);

        // UserRepository сам создаст свой scope с DbContext
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
            _logger.LogWarning("User with id {UserId} not found", id);
        return user;
    }

    public async Task<List<User>> GetAllAsync(int limit = 50)
    {
        _logger.LogDebug("Getting all users (limit {Limit})", limit);
        return await _userRepository.GetAllAsync(limit);
    }

    public async Task<User?> CreateAsync(User user)
    {
        _logger.LogInformation("Creating user {UserName}", user?.Username);
        var created = await _userRepository.AddAsync(user);
        if (!created)
        {
            _logger.LogError("Failed to create user {UserName}", user?.Username);
            return null;
        }

        _logger.LogInformation("User {UserName} created", user.Username);
        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        _logger.LogInformation("Updating user {UserId}", user?.Id);
        var updated = await _userRepository.Update(user);
        if (!updated)
        {
            _logger.LogError("Failed to update user {UserId}", user?.Id);
            return false;
        }

        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        _logger.LogInformation("Deleting user {UserId}", id);
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found for deletion", id);
            return false;
        }

        var deleted = await _userRepository.Delete(user);
        if (!deleted)
        {
            _logger.LogError("Failed to delete user {UserId}", id);
            return false;
        }

        return true;
    }
}
