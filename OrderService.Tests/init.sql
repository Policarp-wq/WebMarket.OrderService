CREATE TABLE order_status(
  status_id SMALLINT PRIMARY KEY,
  code text UNIQUE NOT NULL
);

INSERT INTO order_status (status_id, code) VALUES
(1, 'PROCESSING'),
(2, 'PACKING_UP'),
(3, 'DELIVERING'),
(4, 'DELIVERED'),
(5, 'COMPLETED'),
(6, 'DENIED');

CREATE TABLE delivery_status(
  status_id SMALLINT PRIMARY KEY,
  code text UNIQUE NOT NULL
);
INSERT INTO delivery_status (status_id, code) VALUES
(1, 'DELIVERING_TO'),
(2, 'SORTING'),
(3, 'SENT');


CREATE TABLE checkpoint(
    checkpoint_id serial PRIMARY KEY,
    owner_id integer NOT NULL,
    address text,
    is_delivery_point BOOLEAN NOT NULL,
    location geometry NOT NULL
);

CREATE TABLE customer_order(
    order_id serial PRIMARY KEY,
    customer_id integer NOT NULL,
    product_id integer NOT NULL,
    delivery_point_id integer NOT NULL REFERENCES checkpoint(checkpoint_id) ON DELETE RESTRICT,
    status SMALLINT DEFAULT 1 REFERENCES order_status(status_id) ON DELETE CASCADE,
    track_number VARCHAR(9) not null UNIQUE CHECK (LENGTH(track_number) = 9), /** Performance issues **/
    created_at timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE order_status_story(
  story_id serial PRIMARY KEY,
  order_id integer NOT NULL REFERENCES customer_order(order_id),
  status SMALLINT NOT NULL REFERENCES order_status(status_id) ON DELETE CASCADE,
  change_date timestamp NOT NULL
);

CREATE TABLE order_trace(
  trace_id serial PRIMARY KEY,
  order_id integer NOT NULL REFERENCES customer_order(order_id) ON DELETE CASCADE,
  checkpoint_id integer NOT NULL REFERENCES checkpoint(checkpoint_id),
  delivery_status SMALLINT REFERENCES delivery_status(status_id) ON DELETE CASCADE,
  delivery_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

create or replace function fn_GetClosestPoint(given geometry)
returns table(
	checkpoint_id integer,
	owner_id integer,
	location geometry
) as
$$
begin
	return query
	select sub.checkpoint_id, sub.owner_id, sub.location
	from
		(select checkpoint.checkpoint_id, checkpoint.location, checkpoint.owner_id,
		checkpoint.location <-> ST_SetSRID(given, ST_SRID(checkpoint.location)) as dist
		from checkpoint 
		order by dist asc
		limit 1)
	as sub;
end;
$$ language plpgsql;
