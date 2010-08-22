DROP TABLE IF EXISTS `Errors`;
CREATE TABLE  `Errors` (
  `timestamp` datetime default NULL,
  `code` int(10) unsigned default NULL,
  `ip` varchar(45) default NULL,
  `location` varchar(256) default NULL,
  `message` varchar(256) default NULL,
  `stack` varchar(8000) default NULL,
  `referrer` varchar(256) default NULL,
  `sessionID` varchar(256) default NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `Searches`;
CREATE TABLE  `Searches` (
  `created` datetime default NULL,
  `term` varchar(256) default NULL,
  `sessionId` varchar(256) default NULL,
  `referer` varchar(256) default NULL,
  `numResults` int(10) unsigned default NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `Traces`;
CREATE TABLE  `Traces` (
  `timestamp` datetime NOT NULL,
  `location` varchar(500) default NULL,
  `category` varchar(100) default NULL,
  `message` varchar(1000) default NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

DROP TABLE IF EXISTS `UserErrors`;
CREATE TABLE  `UserErrors` (
  `timestamp` datetime NOT NULL,
  `sessionID` varchar(100) default NULL,
  `location` varchar(200) default NULL,
  `error` varchar(100) NOT NULL,
  `valueEntered` varchar(100) default NULL
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

CREATE TABLE  `UserSessions` (
  `sessionID` varchar(45) NOT NULL,
  `created` datetime NOT NULL,
  `ip` varchar(45) NOT NULL,
  `userAgent` varchar(255) DEFAULT NULL,
  `referrer` varchar(500) DEFAULT NULL,
  `exclude` tinyint(1) NOT NULL DEFAULT '0',
  KEY `SessionID_Index` (`sessionID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;