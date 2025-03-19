// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Pages.Login;

public class InputModel
{
    [Required(ErrorMessage = "Введите логин")]
    public string? Username { get; set; }
    [Required(ErrorMessage = "Введите пароль")]
    public string? Password { get; set; }
    public bool RememberLogin { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Button { get; set; }
}