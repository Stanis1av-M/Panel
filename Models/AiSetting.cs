using System;
using System.Collections.Generic;

namespace panel.Models;

/// <summary>
/// Настройки подключения к нейросети (хранится один актуальный ключ/конфигурация).
/// </summary>
public partial class AiSetting
{
    public int AiSettingId { get; set; }

    /// <summary>
    /// Название провайдера (например "OpenAI", "Anthropic" и т.д.) — на будущее, если подключений будет несколько.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// API-ключ для обращения к нейросети. Пока вводится пользователем вручную в окне чата.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// URL эндпоинта API (на случай если потребуется смена адреса/версии).
    /// </summary>
    public string? ApiUrl { get; set; }

    public DateTime UpdatedAt { get; set; }
}
