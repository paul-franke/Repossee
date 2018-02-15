using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DemoData.Models
{
    [DataContract]

    public partial class StudentDTO
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Passportnumber is required.")]
        [DataMember]
        public string PassportNumber { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int? Mentor_Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [DataMember]
        public string Name { get; set; }

        [Required(ErrorMessage = "Level is required.")]
        [DataMember]
        public int Level_Id { get; set; }

        [DataMember]
        public List<int> CourseIds { get; set; }
    }

}


