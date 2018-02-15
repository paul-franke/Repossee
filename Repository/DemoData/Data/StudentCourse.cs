using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoData.Data
{
    [DataContract]
    public partial class StudentCourse
    {

        [Key]
        [Column(Order = 1)]
        [ForeignKey("Student")]
        public int Student_Id { get; set; }
 
        [DataMember]
        [Key]
        [Column(Order = 2)]
        [ForeignKey("Course")]
        public int Course_Id { get; set; }
        public virtual Student Student { get; set; }
        public virtual Course Course{ get; set; }

      }

}





