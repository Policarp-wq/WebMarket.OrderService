CREATE TYPE order_status AS ENUM ('processing','packing_up', 'delivering', 'delivered', 'completed', 'denied');
CREATE TABLE checkpoint(
    checkpoint_id serial PRIMARY KEY,
    owner_id integer NOT NULL,
    is_delivery_point BOOLEAN NOT NULL,
    location geometry NOT NULL
);

CREATE TABLE customer_order(
    order_id serial PRIMARY KEY,
    customer_id integer NOT NULL,
    product_id integer NOT NULL,
    delivery_point_id integer NOT NULL REFERENCES checkpoint(checkpoint_id) ON DELETE RESTRICT,
    checkpoint_id integer NOT NULL REFERENCES checkpoint(checkpoint_id) ON DELETE RESTRICT, /** Can insert not delivery **/
    status order_status DEFAULT 'processing',
    track_number VARCHAR(9) not null UNIQUE CHECK (LENGTH(track_number) = 9), /** Performance issues **/
    created_at timestamp DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE customer_history(
  id serial PRIMARY KEY,
  cutomer_id integer NOT NULL,
  product_id integer NOT NULL,
  order_date timestamp NOT NULL
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
