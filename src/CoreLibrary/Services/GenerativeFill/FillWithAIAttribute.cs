﻿namespace CoreLibrary.Services.GenerativeFill;

/// <summary>
/// Attribute used to decorate fields that should be filled by AI.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FillWithAIAttribute : Attribute
{
}
