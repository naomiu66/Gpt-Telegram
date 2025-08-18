using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Gpt_Telegram.Utilities.Telegram
{
    public class KeyboardMarkupBuilder
    {
        public InlineKeyboardButton InitializeKeyboardButton(string text, string callbackData)
        {
            return new InlineKeyboardButton
            {
                Text = text,
                CallbackData = callbackData
            };
        }

        public InlineKeyboardMarkup InitializeKeyboardMarkup(List<InlineKeyboardButton> buttons, int buttonsPerRow = 1)
        {
            return new InlineKeyboardMarkup(buttons.Chunk(buttonsPerRow));
        }

        public async Task RemoveKeybordMarkup(ITelegramBotClient botClient, long chatId, int messageId)
        {
            await botClient.EditMessageReplyMarkup(
                chatId: chatId,
                messageId: messageId,
                replyMarkup: null
            );
        }
    }
}
