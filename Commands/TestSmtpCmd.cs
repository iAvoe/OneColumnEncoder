using OneColumnEncoder.Helpers;
using OneColumnEncoder.Models;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace OneColumnEncoder.Commands;

public class TestSmtpCmd(AppConfM appConfM) : AsyncBaseCmd
{
    private readonly AppConfM _appConfM = appConfM;

    protected override async Task ExecuteAsync(object? parameter)
    {
        try
        {
            await SmtpNotificationH.SendTestAsync(_appConfM.Smtp);
            MessageBox.Show("SMTP test message sent.", "SMTP Test", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "SMTP Test Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
