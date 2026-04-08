using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class TransferByIbanDto
    {
        [Required(ErrorMessage = "Receiver IBAN is required.")]
        [StringLength(34, MinimumLength = 10, ErrorMessage = "Receiver IBAN is not valid.")]
        public string? ReceiverIban { get; set; }

        [Required(ErrorMessage = "Receiver first name is required.")]
        [StringLength(50, ErrorMessage = "Receiver first name cannot exceed 50 characters.")]
        public string? ReceiverFirstName { get; set; }

        [Required(ErrorMessage = "Receiver last name is required.")]
        [StringLength(50, ErrorMessage = "Receiver last name cannot exceed 50 characters.")]
        public string? ReceiverLastName { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public string? Amount { get; set; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a transaction type.")]
        public int PaymentTypeId { get; set; }
    }
}