using System;
using System.Threading.Tasks;
using Xunit;

namespace GitHubLabelSync.Tests
{
    public class ProgramTests
    {
        [Fact]
        public async Task CanRunProgram()
        {
            //arrange
            var args = Array.Empty<string>();
            
            //act
            var program = Program.Main(args);
            
            //assert
            await program;
            Assert.True(program.IsCompleted);
        }
    }
}