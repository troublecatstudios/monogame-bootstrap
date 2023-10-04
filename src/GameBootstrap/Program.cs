using GameBootstrap;
using Serilog;
using Troublecat;

try {
    var program = new TroublecatProgram<BootstrappedGame>();
    await program.RunAsync();
} catch(Exception ex) {
    Console.WriteLine("Whoops! Something went wrong. \n" + ex.ToString());
} finally {
    Log.CloseAndFlush();
}
