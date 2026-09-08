CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

DO $$ 
DECLARE
    country_id uuid := uuid_generate_v4();
    user1_id uuid := uuid_generate_v4();
    user4_id uuid := uuid_generate_v4();
    order_id uuid := uuid_generate_v4();
BEGIN
    INSERT INTO "Countries" ("Id", "Name", "Currency", "DefaultLanguage", "IsDefault", "IsActive", "IsDeleted", "CreatedAt")
    VALUES (country_id, 'Test Country', 'USD', 'en', true, true, false, NOW());

    -- Normal user, BCrypt legacy hash
    INSERT INTO "Users" ("Id", "Name", "Email", "Phone", "PasswordHash", "CountryId", "Role", "PreferredLanguage", "CreatedAt", "IsActive", "IsDeleted")
    VALUES (user1_id, 'John Doe', 'john@example.com', '+1234567890', '$2a$11$WvN2L65Z4Q2U.E0.a2O77uG9c88O559B.Qo8a8O64R.e1k6z8Zq/m', country_id, 0, 'en', NOW(), true, false);

    -- Edge case: long email
    INSERT INTO "Users" ("Id", "Name", "Email", "Phone", "PasswordHash", "CountryId", "Role", "PreferredLanguage", "CreatedAt", "IsActive", "IsDeleted")
    VALUES (user4_id, 'Edge Case', 'this_is_a_very_long_email_address_to_test_limits_in_the_database@example.com', '123123123', '$2a$11$WvN2L65Z4Q2U.E0.a2O77uG9c88O559B.Qo8a8O64R.e1k6z8Zq/m', country_id, 3, 'en', NOW(), true, false);

    -- Seed related data for user1
    INSERT INTO "Orders" ("Id", "OrderNumber", "UserId", "TotalAmount", "Status", "PaymentMethod", "PaymentStatus", "Address", "Phone", "IsActive", "IsDeleted", "CreatedAt", "CountryId")
    VALUES (order_id, 'ORD-123', user1_id, 100.0, 0, 0, 0, '123 Main St', '123456', true, false, NOW(), country_id);

    INSERT INTO "OrderEditLogs" ("Id", "OrderId", "EditedBy", "EditorId", "FieldName", "EditedAt", "CreatedAt", "IsActive", "IsDeleted")
    VALUES (uuid_generate_v4(), order_id, user1_id, user1_id, 'Status', NOW(), NOW(), true, false);

END $$;
