using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;


namespace DemoData.Data
{
        [Table("Student")]
        [DataContract]
    public partial class Student
    {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
            public Student()
            {
                InternShip = new HashSet<InternShip>();
               StudentCourse = new HashSet<StudentCourse>();
            }
            [Required]
            [DataMember]
            [Key]
            public int Id { get; set; }

            [Required(ErrorMessage = "Passportnumber is required.")]
            [DataMember]
            public string PassportNumber { get; set; }

            [Required(ErrorMessage = "Description is required.")]
            [DataMember]
            public string Description { get; set; }

            [DataMember]
            [ForeignKey("Mentor")]
            public int? Mentor_Id { get; set; }

            [Required(ErrorMessage = "Name is required.")]
            [DataMember]
            public string Name { get; set; }

            [Required(ErrorMessage = "Level is required.")]
            [DataMember]
            [ForeignKey("Level")]
            public int Level_Id { get; set; }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
            public virtual ICollection<InternShip> InternShip { get; set; }

            public virtual Level Level { get; set; }

            public virtual Mentor Mentor { get; set; }

           
                   
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]

       public virtual HashSet<StudentCourse> StudentCourse { get; set; }

        }

}




