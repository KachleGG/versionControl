using System.Runtime.InteropServices;
namespace UpdateMGR {

    class Program {
        
        static void Main(string[] args) {
            // Initialize updater and try to update
            Updater updater = new Updater("KachleGG", "versionControl", "1.2.0", "Updatr");
            updater.Update();
        }
    }
}