using Azure.Messaging.ServiceBus;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FuncionEncolar040123
{
    public class Servicio : IServicio
    {
        private readonly ServiceBusClient _serviceBusClient;
        public Servicio(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient;
        }

        public async Task SendMessageAsync(ClienteModel modelMessage)
        {
            ServiceBusSender sender = _serviceBusClient.CreateSender("primerazure");
            var body = System.Text.Json.JsonSerializer.Serialize(modelMessage);
            var sbMessage = new ServiceBusMessage(body);
            await sender.SendMessageAsync(sbMessage);
        }

        public async Task GetNewData(CancellationToken stoppingToken)
        {
            string cadena = "Data Source=DESKTOP-LORI9GP; Initial Catalog=AzureCliente; user id=sa;password=123456; TrustServerCertificate=True;";
            string cadenaBdAzure = "Server=tcp:servidorbdazurecliente.database.windows.net,1433;Initial Catalog=AzureCliente;Persist Security Info=False;User ID=dante.lopez@softtek.com@servidorbdazurecliente;Password=Azuredb10;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            //string cadenaHibrida = "Endpoint=sb://encoladoservicebus.servicebus.windows.net/;SharedAccessKeyName=defaultListener;SharedAccessKey=1/xsV9gMIYC8lBjCiZNj1kOkO13mXMBjtsganKPurKs=;EntityPath=SqlHybridConnection";
            //string cadena = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultDbConnection"].ConnectionString;


            SqlConnection con = new SqlConnection(cadenaBdAzure);
            con.Open();
            SqlCommand comando = new SqlCommand("Select Id, Nombre, Apellido, NroDocumento, ClienteId, EsNuevo From [ClienteNuevo] Where EsNuevo = 1", con);

            var clientes = new List<ClienteModelId>();
            using (SqlDataReader reader = comando.ExecuteReader())
            {
                while (reader.Read())
                {
                    var cliente = new ClienteModel()
                    {
                        Nombre = reader["Nombre"]?.ToString(),
                        Apellido = reader["Apellido"]?.ToString(),
                        NroDocumento = (int)reader["NroDocumento"]
                    };

                    await SendMessageAsync(cliente);

                    var clienteId = new ClienteModelId()
                    {
                        Nombre = reader["Nombre"]?.ToString(),
                        Apellido = reader["Apellido"]?.ToString(),
                        NroDocumento = (int)reader["NroDocumento"],
                        Id = Int32.Parse(reader["Id"]?.ToString())
                    };
                    clientes.Add(clienteId);
                }
            }
            using (SqlCommand comando2 = con.CreateCommand())
            {
                foreach (var item in clientes)
                {
                    comando2.CommandText = $"Update ClienteNuevo Set EsNuevo = 0 Where Id = {item.Id}";
                    comando2.ExecuteNonQuery();
                }
            }
            con.Close();
        }
    }
}
