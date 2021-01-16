using Sync_and_Edit.Menu;
using Windows.UI.Xaml.Controls;


namespace Sync_and_Edit.InfoPage
{
    public sealed partial class About : Page
    {
        public About()
        {
            this.InitializeComponent();
            Info_Page.my_list_box.SelectedIndex = 2;
            Text();

        }

        public void Text()
        {
            Zagalovoc.Text = "Sync and Edit v 0.3 (от 13.06.2018)";
            Main_Text.LineHeight = 40;
            Main_Text.Text = "В текущей версии приложения были произведены следующие изменения: \n" +
                "  -  исправлено обновление аудиотеки на странице источников; \n" +
                "  -  исправлено отображение синхронизированных песен; \n" +
                "  -  исправлено отображение уже перенесенных песен в синхронизации; \n" +
                "  -  добавлены значения для вывода на страницу статистика \n" +
                "  -  исправлены замеченные ошибки в работе программы. \n";
        }
    }
}
