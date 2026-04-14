using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AmusementPark.Models
{
    public class WebSocketMessage
    {
        public string Action { get; set; } = string.Empty;   // Название операции
        public JsonElement? Payload { get; set; }            // Данные (зависит от Action)
        public string? Error { get; set; }

        public static class WebSocketActions
        {
            // Аккаунт
            public const string GetUsers = "GET_USERS";
            public const string AddUser = "ADD_USER";
            public const string UpdateUser = "UPDATE_USER";
            public const string DeleteUser = "DELETE_USER";

            public const string GetRoles = "GET_ROLES";
            public const string AddRole = "ADD_ROLE";
            public const string UpdateRole = "UPDATE_ROLE";
            public const string DeleteRole = "DELETE_ROLE";

            public const string GetProfiles = "GET_PROFILES";
            public const string AddProfile = "ADD_PROFILE";
            public const string UpdateProfile = "UPDATE_PROFILE";
            public const string DeleteProfile = "DELETE_PROFILE";

            // Билеты
            public const string GetTickets = "GET_TICKETS";
            public const string AddTicket = "ADD_TICKET";
            public const string UpdateTicket = "UPDATE_TICKET";
            public const string DeleteTicket = "DELETE_TICKET";

            // Платежи
            public const string GetPayments = "GET_PAYMENTS";
            public const string AddPayment = "ADD_PAYMENT";
            public const string UpdatePayment = "UPDATE_PAYMENT";
            public const string DeletePayment = "DELETE_PAYMENT";

            // Посещения парка
            public const string GetVisits = "GET_VISITS";
            public const string AddVisit = "ADD_VISIT";
            public const string UpdateVisit = "UPDATE_VISIT";
            public const string DeleteVisit = "DELETE_VISIT";

            // Аттракционы
            public const string GetRides = "GET_RIDES";
            public const string AddRide = "ADD_RIDE";
            public const string UpdateRide = "UPDATE_RIDE";
            public const string DeleteRide = "DELETE_RIDE";

            public const string GetRideGroups = "GET_RIDE_GROUPS";
            public const string AddRideGroup = "ADD_RIDE_GROUP";
            public const string UpdateRideGroup = "UPDATE_RIDE_GROUP";
            public const string DeleteRideGroup = "DELETE_RIDE_GROUP";

            public const string GetRideVisits = "GET_RIDE_VISITS";
            public const string AddRideVisit = "ADD_RIDE_VISIT";
            public const string UpdateRideVisit = "UPDATE_RIDE_VISIT";
            public const string DeleteRideVisit = "DELETE_RIDE_VISIT";

            // Тарифы
            public const string GetPrices = "GET_PRICES";
            public const string AddPrice = "ADD_PRICE";
            public const string UpdatePrice = "UPDATE_PRICE";
            public const string DeletePrice = "DELETE_PRICE";

            public const string GetCategories = "GET_CATEGORIES";
            public const string AddCategory = "ADD_CATEGORY";
            public const string UpdateCategory = "UPDATE_CATEGORY";
            public const string DeleteCategory = "DELETE_CATEGORY";

            public const string GetStatuses = "GET_STATUSES";
            public const string AddStatus = "ADD_STATUS";
            public const string UpdateStatus = "UPDATE_STATUS";
            public const string DeleteStatus = "DELETE_STATUS";

            // Общие
            public const string Ping = "PING";
        }
    }
}