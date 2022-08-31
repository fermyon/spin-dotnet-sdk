CREATE TABLE pets (
  id integer PRIMARY KEY,
  name varchar(80) not null,
  picture bytea
);

CREATE TABLE toys (
  id integer PRIMARY KEY,
  owner_id integer REFERENCES pets (id),
  count integer,
  description varchar(200) not null,
  picture bytea
);

