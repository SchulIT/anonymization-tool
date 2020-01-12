using AnonymizationTool.Data.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymizationTool.Anonymization
{
    public interface IStudentFaker
    {
        void FakeStudent(AnonymousStudent student, int attempt);
    }
}
