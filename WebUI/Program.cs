using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogoCrmTelegramIntegration.Application.Interfaces;
using LogoCrmTelegramIntegration.Application.Services;
using LogoCrmTelegramIntegration.Infrastructure.Persistence;
using LogoCrmTelegramIntegration.Infrastructure.Services;
using LogoCrmTelegramIntegration.Infrastructure.Telegram;
using LogoCrmTelegramIntegration.WebUI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LogoCrmTelegramIntegration.WebUI;

public class Program
{
	private static NotifyIcon _notifyIcon;

	private static WebApplication _app;

	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			SetStartup();
			try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "startup.log"), $"Started at {DateTime.Now}"); } catch { }
			System.Windows.Forms.Application.EnableVisualStyles();
			System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(defaultValue: false);

			var webApplicationBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
			{
				Args = args,
				ContentRootPath = AppContext.BaseDirectory
			});

			int port = 5000;
			if (int.TryParse(webApplicationBuilder.Configuration["ServerPort"], out int p)) port = p;
			webApplicationBuilder.WebHost.UseUrls($"http://0.0.0.0:{port}");

			webApplicationBuilder.Services.AddControllers();

			webApplicationBuilder.Services.AddSingleton<IAppPermissionStore, AppPermissionStore>();
			webApplicationBuilder.Services.AddSingleton<IWebUserStore, WebUserStore>();
			webApplicationBuilder.Services.AddSingleton<ITelegramUserStore, TelegramUserStore>();
			webApplicationBuilder.Services.AddSingleton<IProposalLogStore, ProposalLogStore>();
			webApplicationBuilder.Services.AddSingleton<IProposalRepository, ProposalRepository>();
			webApplicationBuilder.Services.AddSingleton<ILogoErpRepository, LogoErpRepository>();
			webApplicationBuilder.Services.AddSingleton<ICariStatementPdfGenerator, CariStatementPdfGenerator>();
			webApplicationBuilder.Services.AddSingleton<IChequeReportPdfGenerator, ChequeReportPdfGenerator>();
			webApplicationBuilder.Services.AddSingleton<IOrderPdfGenerator, OrderPdfGenerator>();
			webApplicationBuilder.Services.AddSingleton<IStandardPdfGenerator, StandardPdfGenerator>();
			webApplicationBuilder.Services.AddSingleton<IDraftOrderStore, DraftOrderStore>();
			webApplicationBuilder.Services.AddSingleton<IXsltDispatchService, XsltDispatchService>();
			webApplicationBuilder.Services.AddSingleton<IXsltInvoiceService, XsltInvoiceService>();
			webApplicationBuilder.Services.AddSingleton<IVoiceSpeechTranscriber, VoiceSpeechTranscriber>();
			webApplicationBuilder.Services.AddSingleton<IProposalAppService, ProposalAppService>();
			webApplicationBuilder.Services.AddSingleton<ITelegramBotManager, TelegramBotManager>();
			webApplicationBuilder.Services.AddHttpClient();
			webApplicationBuilder.Services.AddSingleton<ISmsService, NetGsmSmsService>();
			webApplicationBuilder.Services.AddSingleton<IEmailService, SmtpEmailService>();
			webApplicationBuilder.Services.AddSingleton<IEBelgeWebServiceClient, EBelgeWebServiceClient>();
			webApplicationBuilder.Services.AddSingleton<IGibSchematronValidator, GibSchematronValidator>();
			webApplicationBuilder.Services.AddSingleton<IEBelgeDispatcherService, EBelgeDispatcherService>();
			webApplicationBuilder.Services.AddSingleton<IDynamicSqlExecutor, DynamicSqlExecutor>();
			webApplicationBuilder.Services.AddSingleton<ICustomReportStore, CustomReportStore>();
			webApplicationBuilder.Services.AddSingleton<IReportHistoryStore, ReportHistoryStore>();
			webApplicationBuilder.Services.AddSingleton<IAiService, GeminiAiService>();
			webApplicationBuilder.Services.AddSingleton<IAiAssistantService, AiAssistantService>();
			webApplicationBuilder.Services.AddSingleton<IExecutiveReportService, ExecutiveReportService>();
			webApplicationBuilder.Services.AddHostedService<TelegramHostedService>();
			webApplicationBuilder.Services.AddHostedService<ExecutiveReportHostedService>();
			webApplicationBuilder.Services.AddCors(delegate(CorsOptions options)
			{
				options.AddPolicy("AllowAll", delegate(CorsPolicyBuilder policy)
				{
					policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
				});
			});
			WebApplication webApplication = webApplicationBuilder.Build();
			webApplication.UseCors("AllowAll");
			DefaultFilesOptions defaultFilesOptions = new DefaultFilesOptions();
			defaultFilesOptions.DefaultFileNames.Clear();
			defaultFilesOptions.DefaultFileNames.Add("login.html");
			webApplication.UseDefaultFiles(defaultFilesOptions);
			webApplication.UseStaticFiles(new StaticFileOptions
			{
				OnPrepareResponse = delegate(StaticFileResponseContext ctx)
				{
					ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
					ctx.Context.Response.Headers.Append("Pragma", "no-cache");
					ctx.Context.Response.Headers.Append("Expires", "0");
				}
			});
			webApplication.UseAuthorization();
			webApplication.MapControllers();
			_app = webApplication;
			// Start Tray Icon in an STA thread with a message loop
			Thread trayThread = new Thread(() =>
			{
				try
				{
					System.Windows.Forms.Application.EnableVisualStyles();
					System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
					InitializeTrayIcon();
					System.Windows.Forms.Application.Run();
				}
				catch (Exception ex)
				{
					try { File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "startup_error.log"), "TrayIcon Error: " + ex.ToString() + "\n"); } catch { }
				}
			});
			trayThread.SetApartmentState(ApartmentState.STA);
			trayThread.IsBackground = true;
			trayThread.Start();

			// Web arayüzünü otomatik aç
			Task.Run(async () =>
			{
				await Task.Delay(1800);
				try
				{
					Process.Start(new ProcessStartInfo("http://localhost:5000") { UseShellExecute = true });
				}
				catch { }
			});

			_app.Run();
		}
		catch (Exception ex)
		{
			try
			{
				File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "startup_error.log"), "Main Error: " + ex.ToString() + "\n");
			}
			catch { }
			MessageBox.Show("Logo Asistan Bot başlatılırken hata oluştu:\n\n" + ex.Message, "Başlatma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private static void InitializeTrayIcon()
	{
		ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
		ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("Ayarları Aç (Web Arayüzü)");
		toolStripMenuItem.Click += delegate
		{
			OpenWebUi();
		};
		contextMenuStrip.Items.Add(toolStripMenuItem);
		contextMenuStrip.Items.Add(new ToolStripSeparator());
		ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("Çıkış");
		toolStripMenuItem2.Click += async delegate
		{
			await ExitApplication();
		};
		contextMenuStrip.Items.Add(toolStripMenuItem2);
		_notifyIcon = new NotifyIcon
		{
			Icon = SystemIcons.Application,
			ContextMenuStrip = contextMenuStrip,
			Visible = true,
			Text = "Logo CRM Telegram Bot"
		};
		_notifyIcon.DoubleClick += delegate
		{
			OpenWebUi();
		};
	}

	private static void OpenWebUi()
	{
		try
		{
			string text = "http://localhost:5000/login.html";
			if (_app.Urls.Count > 0)
			{
				text = _app.Urls.FirstOrDefault((string u) => u.StartsWith("http://")) ?? (_app.Urls.FirstOrDefault() + "/login.html");
				if (!text.EndsWith("/login.html"))
				{
					text = text.TrimEnd('/') + "/login.html";
				}
			}
			Process.Start(new ProcessStartInfo
			{
				FileName = text,
				UseShellExecute = true
			});
		}
		catch
		{
			MessageBox.Show("Web arayüzü açılamadı. Tarayıcınızdan http://localhost:5000 adresine gidebilirsiniz.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
	}

	private static void SetStartup()
	{
		try
		{
			string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
			// Eğer dotnet.exe üzerinden çalışıyorsa doğrudan bat dosyasını kullan
			string startupPath;
			if (exePath.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase))
			{
				// dotnet CLI üzerinden çalışıyorsa start-bot.bat kullan
				string batPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "start-bot.bat");
				if (!File.Exists(batPath))
					batPath = Path.Combine(AppContext.BaseDirectory, "start-bot.bat");
				startupPath = batPath;
			}
			else
			{
				// Doğrudan .exe olarak çalışıyorsa exe yolunu kullan
				startupPath = exePath;
			}

			if (!string.IsNullOrWhiteSpace(startupPath) && File.Exists(startupPath))
			{
				Microsoft.Win32.RegistryKey? rk = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
				rk?.SetValue("LogoAsistanBot", $"\"{startupPath}\"");
			}
		}
		catch { }
	}

	private static async Task ExitApplication()
	{
		try
		{
			if (_notifyIcon != null)
			{
				_notifyIcon.Visible = false;
				_notifyIcon.Dispose();
			}
			if (_app != null)
			{
				await _app.StopAsync();
			}
		}
		catch { }
		Environment.Exit(0);
	}
}
