#=====================================================================
# DummyTraining (Simple Strength Bar Training)
#=====================================================================
import pygame

from core import runtime_globals
from core.animation import PetFrame
from core.combat.combat_constants import ALERT_DURATION_FRAMES, IMPACT_DURATION_FRAMES, RESULT_SCREEN_FRAMES, WAIT_ATTACK_READY_FRAMES
from core.constants import *
from core.utils.pet_utils import distribute_pets_evenly, get_training_targets
from core.utils.pygame_utils import blit_with_shadow, load_attack_sprites, sprite_load_percent
from core.utils.scene_utils import change_scene

class Training:
    """
    Training mode where players build up strength by holding a bar.
    """

    def __init__(self) -> None:
        self.phase = "alert"
        self.frame_counter = 0

        self.attack_positions = []
        self.attack_phase = 1
        self.attack_waves = []
        self.current_wave_index = 0
        self.flash_frame = 0
        self.impact_counter = 0
        self.attacks_prepared = False

        # Sprite caching
        self._sprite_cache = {}
        self._pet_sprite_cache = {}
        self.pet_state = None

        # Load and cache all sprites once
        self._sprite_cache['ready'] = sprite_load_percent(READY_SPRITE_PATH, 100, keep_proportion=True, base_on="width", alpha=False)
        self._sprite_cache['go'] = sprite_load_percent(GO_SPRITE_PATH, 100, keep_proportion=True, base_on="width", alpha=False)
        self._sprite_cache['bar_piece'] = sprite_load_percent(BAR_PIECE_PATH, percent=(int(12 * UI_SCALE) / SCREEN_HEIGHT) * 100, keep_proportion=True, base_on="height")
        self._sprite_cache['training_max'] = sprite_load_percent(TRAINING_MAX_PATH, percent=(int(60 * UI_SCALE) / SCREEN_HEIGHT) * 100, keep_proportion=True, base_on="height")
        self._sprite_cache['bar_back'] = sprite_load_percent(BAR_BACK_PATH, percent=(int(170 * UI_SCALE) / SCREEN_HEIGHT) * 100, keep_proportion=True, base_on="height")
        self._sprite_cache['battle1'] = sprite_load_percent(BATTLE1_PATH, 100, keep_proportion=True, base_on="width", alpha=False)
        self._sprite_cache['battle2'] = sprite_load_percent(BATTLE2_PATH, 100, keep_proportion=True, base_on="width", alpha=False)
        self._sprite_cache['bad'] = sprite_load_percent(BAD_SPRITE_PATH, 100, keep_proportion=True, base_on="width", alpha=False)
        self._sprite_cache['good'] = sprite_load_percent(GOOD_SPRITE_PATH, 100, keep_proportion=True, base_on="width", alpha=False)
        self._sprite_cache['great'] = sprite_load_percent(GREAT_SPRITE_PATH, 100, keep_proportion=True, base_on="width", alpha=False)
        self._sprite_cache['excellent'] = sprite_load_percent(EXCELLENT_SPRITE_PATH, 100, keep_proportion=True, base_on="width", alpha=False)

        self.attack_jump = 0
        self.attack_forward = 0
        self.attack_frame = None
        self.attack_sprites = load_attack_sprites()
        self.pets = get_training_targets()

    def get_sprite(self, key):
        return self._sprite_cache[key]

    def update(self):
        if self.phase == "alert":
            self.update_alert_phase()
        elif self.phase == "charge":
            self.update_charge_phase()
        elif self.phase == "wait_attack":
            self.update_wait_attack_phase()
        elif self.phase == "attack_move":
            self.move_attacks()
        elif self.phase == "impact":
            self.update_impact_phase()
        elif self.phase == "result":
            self.update_result_phase()
        self.frame_counter += 1

    def update_alert_phase(self):
        if self.frame_counter == int(30 * (FRAME_RATE / 30)):
            runtime_globals.game_sound.play("happy")
        if self.frame_counter >= ALERT_DURATION_FRAMES:
            self.phase = "charge"
            self.frame_counter = 0
            self.bar_timer = pygame.time.get_ticks()

    def update_charge_phase(self):
        pass

    def update_wait_attack_phase(self):
        self.attack_frame = self.animate_attack(20)
        if self.frame_counter >= WAIT_ATTACK_READY_FRAMES:
            self.attack_frame = None
            self.phase = "attack_move"
            self.frame_counter = 0
            runtime_globals.game_sound.play("attack")

    def animate_attack(self, delay=0):
        appear_frame = int(delay * (FRAME_RATE / 30))
        anim_window = int(20 * (FRAME_RATE / 30))
        anim_start = appear_frame - anim_window
        anim_end = appear_frame

        progress = 0
        if anim_start <= self.frame_counter < anim_end:
            progress = (self.frame_counter - anim_start) / max(1, (anim_end - anim_start))
            if progress < 0.5:
                self.attack_forward += 1 * (30 / FRAME_RATE)
                if progress < 0.25:
                    self.attack_jump += 1 * (30 / FRAME_RATE)
                else:
                    self.attack_jump -= 1 * (30 / FRAME_RATE)
            else:
                self.attack_forward -= 1 * (30 / FRAME_RATE)
        else:
            self.attack_forward = 0
            self.attack_jump = 0

        train2_frames = 6 * (FRAME_RATE / 30)
        if delay == 20:
            if self.frame_counter > anim_end - train2_frames:
                frame_enum = PetFrame.TRAIN2
            else:
                frame_enum = PetFrame.TRAIN1
        else:
            if (self.frame_counter > anim_end - train2_frames) or (self.frame_counter < train2_frames):
                frame_enum = PetFrame.TRAIN2
            else:
                frame_enum = PetFrame.TRAIN1
        return frame_enum

    def update_impact_phase(self):
        self.flash_frame += 1
        if self.flash_frame >= IMPACT_DURATION_FRAMES:
            self.phase = "result"
            self.frame_counter = 0

    def update_result_phase(self):
        if self.frame_counter >= RESULT_SCREEN_FRAMES:
            self.finish_training()

    def move_attacks(self):
        pass

    def finish_training(self):
        won = self.check_victory()
        if won:
            runtime_globals.game_sound.play("attack_fail")
        else:
            runtime_globals.game_sound.play("fail")

        for pet in self.pets:
            pet.finish_training(won)

        distribute_pets_evenly()
        change_scene("game")

    def draw(self, screen: pygame.Surface):
        if self.phase == "alert":
            self.draw_alert(screen)
        elif self.phase == "charge":
            self.draw_charge(screen)
        elif self.phase == "wait_attack":
            self.draw_attack_ready(screen)
        elif self.phase == "attack_move":
            self.draw_attack_move(screen)
        elif self.phase == "impact":
            self.draw_impact(screen)
        elif self.phase == "result":
            self.draw_result(screen)

    def _init_pet_sprite_cache(self):
        """
        Pre-scales all pet sprites for each frame_enum and caches them.
        """
        self._pet_sprite_cache = {}
        for pet in self.pets:
            self._pet_sprite_cache[pet] = {}
            for frame_enum in PetFrame:
                sprite = runtime_globals.pet_sprites[pet][frame_enum.value]
                scaled_sprite = pygame.transform.scale(sprite, (OPTION_ICON_SIZE, OPTION_ICON_SIZE))
                self._pet_sprite_cache[pet][frame_enum] = scaled_sprite

    def draw_pets(self, surface, frame_enum=PetFrame.IDLE1):
        # Initialize cache if not present or pets changed
        if not hasattr(self, '_pet_sprite_cache') or set(self._pet_sprite_cache.keys()) != set(self.pets):
            self._init_pet_sprite_cache()

        # Use the correct frame_enum for animation
        if self.attack_frame:
            frame_enum = self.attack_frame
        self.pet_state = frame_enum

        total_pets = len(self.pets)
        available_height = SCREEN_HEIGHT
        spacing = min(available_height // total_pets, OPTION_ICON_SIZE + int(20 * UI_SCALE))
        start_y = (SCREEN_HEIGHT - (spacing * total_pets)) // 2

        for i, pet in enumerate(self.pets):
            pet_sprite = self._pet_sprite_cache[pet][frame_enum]
            x = SCREEN_WIDTH - OPTION_ICON_SIZE - int(16 * UI_SCALE) + int(self.attack_forward * UI_SCALE)
            y = start_y + i * spacing - int(self.attack_jump * UI_SCALE)
            blit_with_shadow(surface, pet_sprite, (x, y))

    def draw_alert(self, screen):
        center_y = SCREEN_HEIGHT // 2 - self.get_sprite('ready').get_height() // 2
        blit_with_shadow(screen, self.get_sprite('ready'), (0, center_y))

    def draw_attack_ready(self, surface):
        self.draw_pets(surface, PetFrame.ATK1)

    def draw_charge(self, surface):
        pass

    def draw_attack_move(self, surface):
        pass

    def draw_impact(self, screen):
        flash = self.get_sprite('battle1') if (self.flash_frame // int(2 * (FRAME_RATE / 30))) % 2 == 0 else self.get_sprite('battle2')
        flash = pygame.transform.scale(flash, (SCREEN_WIDTH, SCREEN_HEIGHT))
        screen.blit(flash, (0, 0))

    def draw_result(self, surface):
        pass

    def handle_event(self, input_action):
        if self.phase == "charge" and input_action == "A":
            runtime_globals.game_sound.play("menu")
            self.strength = min(self.strength + 1, self.bar_level)
        elif self.phase in ["wait_attack", "attack_move", "impact", "result"] and input_action in ["B", "START"]:
            self.finish_training()
        elif self.phase in ["alert", "charge"] and input_action in ["B", "START"]:
            runtime_globals.game_sound.play("cancel")
            change_scene("game")