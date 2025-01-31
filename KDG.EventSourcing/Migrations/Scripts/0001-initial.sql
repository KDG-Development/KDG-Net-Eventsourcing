CREATE TABLE _events (
    entity_a_id UUID NOT NULL,
    entity_b_id UUID     NULL,
    entity_c_id UUID     NULL,
    event       VARCHAR NOT NULL,
    data        JSONB NOT NULL,
    delta       JSONB,
    occurred    TIMESTAMP WITH TIME ZONE NOT NULL,
    user_id     UUID NOT NULL,

    CONSTRAINT PK__events PRIMARY KEY (entity_a_id,event,occurred,user_id)
);