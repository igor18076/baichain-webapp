using System.ComponentModel.DataAnnotations;

namespace WebApplication2.Models
{
    public class ContactFormViewModel
    {
        [Required]
        [Display(Name = "Имя")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Ваш вопрос")]
        public string Message { get; set; } = string.Empty;

        [Display(Name = "Согласие на обработку ПДн")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Необходимо согласие на обработку персональных данных.")]
        public bool Agree { get; set; }
    }
}
