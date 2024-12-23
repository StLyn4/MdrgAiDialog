using System.Threading.Tasks;

namespace MdrgAiDialog.AiProviders;

public interface IAiProvider {
  void SetSystemMessage(string message);
  Task<string> SendMessage(string message);
  void ResetChat();
}
