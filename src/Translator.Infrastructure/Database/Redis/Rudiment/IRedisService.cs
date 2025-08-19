﻿namespace Translator.Infrastructure.Database.Redis.Rudiment;

public interface IRedisService
{
    Task<string?> GetAsync(string key);
    Task SetAsync<T>(string key, T value);
    Task RemoveAsync(string key);
    Task ListPushAsync(string key, string value);
    Task<string[]> ListRangeAsync(string key, int start = 0, int stop = 10);
    Task<long> ListLengthAsync(string key);
    Task ListRemoveAsync(string key, string value);
}

