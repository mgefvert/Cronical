-- Table that contains the job queue
CREATE TABLE `jobs` (
	`id`      INT(11)             NOT NULL AUTO_INCREMENT,   -- Sequential ID
	`time`    DATETIME            NULL DEFAULT NULL,         -- Time to run the job, or NULL to run immediately
	`tag`     VARCHAR(255)        NULL DEFAULT NULL,         -- Custom tag ID; set to track or identify jobs as needed
	`command` TEXT                NOT NULL,                  -- Command to execute
	`owner`   BIGINT(30) UNSIGNED NULL DEFAULT NULL,         -- Field used by Cronical to track execution process (do not touch!)
	PRIMARY KEY (`id`),
	INDEX `owner` (`owner`)
);

-- Table that contains the result of the jobs; can be trimmed or truncated as needed
CREATE TABLE `jobs_archive` (
	`id`         INT(11)      NOT NULL AUTO_INCREMENT,       -- Sequential ID (not necessarily the same as the jobs ID)
	`time`       DATETIME     NULL DEFAULT NULL,             -- Time when executed
	`tag`        VARCHAR(255) NULL DEFAULT NULL,             -- Custom tag ID
	`command`    TEXT         NOT NULL,                      -- Command executed
	`resultcode` INT(11)      NULL DEFAULT NULL,             -- Numerical result code
	`output`     TEXT         NULL,                          -- 64kb of output
	PRIMARY KEY (`id`)
);
