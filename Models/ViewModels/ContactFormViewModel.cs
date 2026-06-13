using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace e_store.Models;

public class ContactFormViewModel
{
    [Required(ErrorMessage = "Името е задолжително")]
    public string FullName { get; set; }
    
    [Required(ErrorMessage = "Е-поштата е задолжителна")]
    [EmailAddress(ErrorMessage = "Внесете валидна е-пошта")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Насловот е задолжителен")]
    public string Subject { get; set; }
    
    [Required(ErrorMessage = "Пораката е задолжителна")]
    public string Message { get; set; }
}