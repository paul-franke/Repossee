using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;


namespace DemoData.Data
{
 
    [Table("Course")]
    [DataContract]
    public partial class Course
    {
 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Course()
        {
          StudentCourse = new HashSet<StudentCourse>();
         }

        [DataMember]
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [DataMember]
        public string MailAddress { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual HashSet<StudentCourse> StudentCourse { get; set; }

    }
}
