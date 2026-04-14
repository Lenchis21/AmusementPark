using AmusementPark.Models;
using AmusementPark.Service;
using AmusementPark.Таблицы;
using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static AmusementPark.Models.WebSocketMessage;

namespace AmusementPark
{
    public class WebSocketHandler
    {
        private readonly DatabaseService _dbService;

        public WebSocketHandler(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task HandleAsync(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var cancellationToken = context.RequestAborted;

            try
            {
                while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellationToken);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        var response = await ProcessMessageAsync(message);
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
        }

        private async Task<string> ProcessMessageAsync(string jsonMessage)
        {
            var response = new WebSocketMessage();
            try
            {
                var request = JsonSerializer.Deserialize<WebSocketMessage>(jsonMessage, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (request == null || string.IsNullOrEmpty(request.Action))
                {
                    response.Error = "Invalid message format";
                    return JsonSerializer.Serialize(response);
                }

                // Маршрутизация действий
                switch (request.Action.ToUpper())
                {
                    // Пинг
                    case WebSocketActions.Ping:
                        response.Payload = JsonDocument.Parse("{\"pong\": true}").RootElement;
                        break;

                    // Роли
                    case WebSocketActions.GetRoles:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetRoles());
                        break;
                    case WebSocketActions.AddRole:
                        {
                            var role = request.Payload?.Deserialize<Role>();
                            if (role == null) throw new ArgumentException("Role data missing");
                            _dbService.AddRole(role);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = role.Id });
                        }
                        break;
                    case WebSocketActions.UpdateRole:
                        {
                            var data = request.Payload?.Deserialize<UpdateRoleDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateRole(data.Id, data.Name);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteRole:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteRole(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Пользователи
                    case WebSocketActions.GetUsers:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetUsers());
                        break;
                    case WebSocketActions.AddUser:
                        {
                            var user = request.Payload?.Deserialize<User>();
                            if (user == null) throw new ArgumentException("User data missing");
                            _dbService.AddUser(user);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = user.Id });
                        }
                        break;
                    case WebSocketActions.UpdateUser:
                        {
                            var data = request.Payload?.Deserialize<UpdateUserDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateUser(data.Id, data.Email, data.Password, data.RoleId);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteUser:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteUser(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Профили
                    case WebSocketActions.GetProfiles:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetProfiles());
                        break;
                    case WebSocketActions.AddProfile:
                        {
                            var profile = request.Payload?.Deserialize<Profile>();
                            if (profile == null) throw new ArgumentException("Profile data missing");
                            _dbService.AddProfile(profile);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = profile.Id });
                        }
                        break;
                    case WebSocketActions.UpdateProfile:
                        {
                            var data = request.Payload?.Deserialize<UpdateProfileDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            DateOnly? birth = data.BirthDate != null ? DateOnly.FromDateTime(data.BirthDate.Value) : null;
                            bool ok = _dbService.UpdateProfile(data.Id, data.FirstName, data.LastName, birth, data.Phone);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteProfile:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteProfile(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Билеты
                    case WebSocketActions.GetTickets:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetTickets());
                        break;
                    case WebSocketActions.AddTicket:
                        {
                            var data = request.Payload?.Deserialize<AddTicketDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            var ticket = _dbService.AddTicket(data.ProfileId, data.PriceId, data.StartTime, data.EndTime);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = ticket.Id });
                        }
                        break;
                    case WebSocketActions.UpdateTicket:
                        {
                            var data = request.Payload?.Deserialize<UpdateTicketDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateTicket(data.Id, data.StartTime, data.EndTime);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteTicket:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteTicket(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Платежи
                    case WebSocketActions.GetPayments:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetPayments());
                        break;
                    case WebSocketActions.AddPayment:
                        {
                            var payment = request.Payload?.Deserialize<Payment>();
                            if (payment == null) throw new ArgumentException("Payment data missing");
                            _dbService.AddPayment(payment);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = payment.Id });
                        }
                        break;
                    case WebSocketActions.UpdatePayment:
                        {
                            var data = request.Payload?.Deserialize<UpdatePaymentDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdatePayment(data.Id, data.Amount, data.PaymentDate);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeletePayment:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeletePayment(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Посещения парка
                    case WebSocketActions.GetVisits:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetVisits());
                        break;
                    case WebSocketActions.AddVisit:
                        {
                            var visit = request.Payload?.Deserialize<Visit>();
                            if (visit == null) throw new ArgumentException("Visit data missing");
                            _dbService.AddVisit(visit);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = visit.Id });
                        }
                        break;
                    case WebSocketActions.UpdateVisit:
                        {
                            var data = request.Payload?.Deserialize<UpdateVisitDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateVisit(data.Id, data.EntryTime, data.ExitTime);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteVisit:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteVisit(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Аттракционы
                    case WebSocketActions.GetRides:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetRides());
                        break;
                    case WebSocketActions.AddRide:
                        {
                            var ride = request.Payload?.Deserialize<Ride>();
                            if (ride == null) throw new ArgumentException("Ride data missing");
                            _dbService.AddRide(ride);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = ride.Id });
                        }
                        break;
                    case WebSocketActions.UpdateRide:
                        {
                            var data = request.Payload?.Deserialize<UpdateRideDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateRide(data.Id, data.Name, data.GroupId, data.MinAge, data.Duration);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteRide:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteRide(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Группы аттракционов
                    case WebSocketActions.GetRideGroups:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetRideGroups());
                        break;
                    case WebSocketActions.AddRideGroup:
                        {
                            var group = request.Payload?.Deserialize<RideGroup>();
                            if (group == null) throw new ArgumentException("RideGroup data missing");
                            _dbService.AddRideGroup(group);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = group.Id });
                        }
                        break;
                    case WebSocketActions.UpdateRideGroup:
                        {
                            var data = request.Payload?.Deserialize<UpdateRideGroupDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateRideGroup(data.Id, data.Name);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteRideGroup:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteRideGroup(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Посещения аттракционов
                    case WebSocketActions.GetRideVisits:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetRideVisits());
                        break;
                    case WebSocketActions.AddRideVisit:
                        {
                            var rv = request.Payload?.Deserialize<RideVisit>();
                            if (rv == null) throw new ArgumentException("RideVisit data missing");
                            _dbService.AddRideVisit(rv);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = rv.Id });
                        }
                        break;
                    case WebSocketActions.UpdateRideVisit:
                        {
                            var data = request.Payload?.Deserialize<UpdateRideVisitDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateRideVisit(data.Id, data.VisitId, data.RideId, data.AccessTime, data.ExitTime);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteRideVisit:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteRideVisit(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Цены
                    case WebSocketActions.GetPrices:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetPrices());
                        break;
                    case WebSocketActions.AddPrice:
                        {
                            var price = request.Payload?.Deserialize<Price>();
                            if (price == null) throw new ArgumentException("Price data missing");
                            _dbService.AddPrice(price);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = price.Id });
                        }
                        break;
                    case WebSocketActions.UpdatePrice:
                        {
                            var data = request.Payload?.Deserialize<UpdatePriceDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdatePrice(data.Id, data.StatusId, data.CategoryId, data.Amount);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeletePrice:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeletePrice(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Категории
                    case WebSocketActions.GetCategories:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetCategories());
                        break;
                    case WebSocketActions.AddCategory:
                        {
                            var cat = request.Payload?.Deserialize<Category>();
                            if (cat == null) throw new ArgumentException("Category data missing");
                            _dbService.AddCategory(cat);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = cat.Id });
                        }
                        break;
                    case WebSocketActions.UpdateCategory:
                        {
                            var data = request.Payload?.Deserialize<UpdateCategoryDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateCategory(data.Id, data.Name);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteCategory:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteCategory(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    // Статусы
                    case WebSocketActions.GetStatuses:
                        response.Payload = JsonSerializer.SerializeToElement(_dbService.GetStatuses());
                        break;
                    case WebSocketActions.AddStatus:
                        {
                            var status = request.Payload?.Deserialize<Status>();
                            if (status == null) throw new ArgumentException("Status data missing");
                            _dbService.AddStatus(status);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = true, id = status.Id });
                        }
                        break;
                    case WebSocketActions.UpdateStatus:
                        {
                            var data = request.Payload?.Deserialize<UpdateStatusDto>();
                            if (data == null) throw new ArgumentException("Invalid data");
                            bool ok = _dbService.UpdateStatus(data.Id, data.Name);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;
                    case WebSocketActions.DeleteStatus:
                        {
                            var id = request.Payload?.GetProperty("id").GetInt32() ?? 0;
                            bool ok = _dbService.DeleteStatus(id);
                            response.Payload = JsonSerializer.SerializeToElement(new { success = ok });
                        }
                        break;

                    default:
                        response.Error = $"Unknown action: {request.Action}";
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Error = ex.Message;
            }

            return JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }

    // DTO классы для десериализации запросов (вынесите в отдельный файл или регион)
    public class UpdateRoleDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
    public class UpdateUserDto { public int Id { get; set; } public string? Email { get; set; } public string? Password { get; set; } public int? RoleId { get; set; } }
    public class UpdateProfileDto { public int Id { get; set; } public string? FirstName { get; set; } public string? LastName { get; set; } public DateTime? BirthDate { get; set; } public string? Phone { get; set; } }
    public class AddTicketDto { public int ProfileId { get; set; } public int PriceId { get; set; } public DateTime StartTime { get; set; } public DateTime EndTime { get; set; } }
    public class UpdateTicketDto { public int Id { get; set; } public DateTime? StartTime { get; set; } public DateTime? EndTime { get; set; } }
    public class UpdatePaymentDto { public int Id { get; set; } public decimal? Amount { get; set; } public DateTime? PaymentDate { get; set; } }
    public class UpdateVisitDto { public int Id { get; set; } public DateTime? EntryTime { get; set; } public DateTime? ExitTime { get; set; } }
    public class UpdateRideDto { public int Id { get; set; } public string? Name { get; set; } public int? GroupId { get; set; } public int? MinAge { get; set; } public int? Duration { get; set; } }
    public class UpdateRideGroupDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
    public class UpdateRideVisitDto { public int Id { get; set; } public int? VisitId { get; set; } public int? RideId { get; set; } public DateTime? AccessTime { get; set; } public DateTime? ExitTime { get; set; } }
    public class UpdatePriceDto { public int Id { get; set; } public int? StatusId { get; set; } public int? CategoryId { get; set; } public decimal? Amount { get; set; } }
    public class UpdateCategoryDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
    public class UpdateStatusDto { public int Id { get; set; } public string Name { get; set; } = string.Empty; }
}