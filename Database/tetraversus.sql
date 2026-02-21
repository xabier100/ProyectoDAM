-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 15-02-2026 a las 19:01:54
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `tetraversus`
--
CREATE DATABASE IF NOT EXISTS `tetraversus` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
USE `tetraversus`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `points`
--

DROP TABLE IF EXISTS `points`;
CREATE TABLE `points` (
  `id_game` int(11) NOT NULL,
  `id_player` int(11) NOT NULL,
  `points` int(11) DEFAULT NULL,
  `level` int(11) DEFAULT NULL,
  `rows_deleted` int(11) DEFAULT NULL,
  `i_pieces` int(11) DEFAULT NULL,
  `j_pieces` int(11) DEFAULT NULL,
  `l_pieces` int(11) DEFAULT NULL,
  `t_pieces` int(11) DEFAULT NULL,
  `s_pieces` int(11) DEFAULT NULL,
  `z_pieces` int(11) DEFAULT NULL,
  `o_pieces` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `points`
--

INSERT INTO `points` (`id_game`, `id_player`, `points`, `level`, `rows_deleted`, `i_pieces`, `j_pieces`, `l_pieces`, `t_pieces`, `s_pieces`, `z_pieces`, `o_pieces`) VALUES
(3, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
(4, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
(6, 100, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7),
(7, 100, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0),
(8, 100, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0),
(9, 100, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0),
(10, 100, 22, 0, 0, 10, 20, 0, 0, 0, 0, 0),
(11, 100, 20, 1, 2, 10, 20, 2, 3, 0, 1, 3),
(12, 100, 20, 1, 2, 10, 20, 2, 3, 0, 2, 0),
(13, 100, 35, 3, 6, 4, 4, 1, 6, 2, 2, 0);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `tokens`
--

DROP TABLE IF EXISTS `tokens`;
CREATE TABLE `tokens` (
  `token_id` int(11) NOT NULL,
  `token` varchar(100) DEFAULT NULL,
  `user_id` int(11) DEFAULT NULL,
  `is_valid` tinyint(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `tokens`
--

INSERT INTO `tokens` (`token_id`, `token`, `user_id`, `is_valid`) VALUES
(68, '57446', 100, 0),
(69, '20421', 100, 0),
(70, '53671', 100, 0),
(71, '97740', 100, 0),
(72, '45934', 100, 0),
(73, '24623', 100, 0),
(74, '60629', 100, 0),
(75, '65577', 100, 0);

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `users`
--

DROP TABLE IF EXISTS `users`;
CREATE TABLE `users` (
  `user_id` int(11) NOT NULL,
  `username` varchar(255) DEFAULT NULL,
  `email` varchar(255) DEFAULT NULL,
  `password` varchar(255) DEFAULT NULL,
  `is_verified` bit(1) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Volcado de datos para la tabla `users`
--

INSERT INTO `users` (`user_id`, `username`, `email`, `password`, `is_verified`) VALUES
(100, 'your-username15', 'xabier.100@hotmail.com', '$2a$11$yJi1E6K2VV/Dx0P5CbFwZeNlNq5dO3jAJQfLqlubdQvbp61Yjta9.', b'1');

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `points`
--
ALTER TABLE `points`
  ADD PRIMARY KEY (`id_game`),
  ADD KEY `points_users_id_fk` (`id_player`);

--
-- Indices de la tabla `tokens`
--
ALTER TABLE `tokens`
  ADD PRIMARY KEY (`token_id`),
  ADD KEY `tokens_users_userId_fk` (`user_id`);

--
-- Indices de la tabla `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`user_id`),
  ADD UNIQUE KEY `username_uk` (`username`),
  ADD UNIQUE KEY `email_uk` (`email`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `points`
--
ALTER TABLE `points`
  MODIFY `id_game` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT de la tabla `tokens`
--
ALTER TABLE `tokens`
  MODIFY `token_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=76;

--
-- AUTO_INCREMENT de la tabla `users`
--
ALTER TABLE `users`
  MODIFY `user_id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=101;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `points`
--
ALTER TABLE `points`
  ADD CONSTRAINT `points_users_id_fk` FOREIGN KEY (`id_player`) REFERENCES `users` (`user_id`);

--
-- Filtros para la tabla `tokens`
--
ALTER TABLE `tokens`
  ADD CONSTRAINT `tokens_users_userId_fk` FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
